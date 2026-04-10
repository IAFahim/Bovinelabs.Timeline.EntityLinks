using BovineLabs.Reaction.Data.Core;
using BovineLabs.Timeline;
using BovineLabs.Timeline.Data;
using Bovinelabs.Timeline.EntityLinks.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Bovinelabs.Timeline.EntityLinks
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct EntityLinkAttachSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var commands = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var graph = new Graph
            {
                Parents = SystemAPI.GetComponentLookup<Parent>(true),
                Targets = SystemAPI.GetComponentLookup<Targets>(true),
                Stores = SystemAPI.GetBufferLookup<EntityLookupStoreData>(true)
            };

            state.Dependency = new ConnectTransition
            {
                Commands = commands,
                Graph = graph,
                AccentLimit = 1,
                Parents = SystemAPI.GetComponentLookup<Parent>(true),
                LocalTransforms = SystemAPI.GetComponentLookup<LocalTransform>(true),
                WorldTransforms = SystemAPI.GetComponentLookup<LocalToWorld>(true),
                PostTransformMatrices = SystemAPI.GetComponentLookup<PostTransformMatrix>(true)
            }.ScheduleParallel(state.Dependency);

            state.Dependency = new DisconnectTransition
            {
                Commands = commands,
                Parents = SystemAPI.GetComponentLookup<Parent>(true),
                PostTransformMatrices = SystemAPI.GetComponentLookup<PostTransformMatrix>(true)
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(ClipActive))]
        [WithNone(typeof(ClipActivePrevious))]
        private partial struct ConnectTransition : IJobEntity
        {
            [WriteOnly] public EntityCommandBuffer.ParallelWriter Commands;
            public Graph Graph;
            [ReadOnly] public byte AccentLimit;
            [ReadOnly] public ComponentLookup<Parent> Parents;
            [ReadOnly] public ComponentLookup<LocalTransform> LocalTransforms;
            [ReadOnly] public ComponentLookup<LocalToWorld> WorldTransforms;
            [ReadOnly] public ComponentLookup<PostTransformMatrix> PostTransformMatrices;

            private void Execute([ChunkIndexInQuery] int chunk, ref EntityLinkAttachState state,
                in EntityLinkAttachConfig config, in TrackBinding binding)
            {
                var origin = binding.Value;
                Graph.Evaluate(origin, config.LinkKey, config.ResolveRule, AccentLimit, out var destination);

                if (destination == Entity.Null)
                {
                    state = new EntityLinkAttachState();
                    return;
                }

                var originParent = Parents.TryGetComponent(origin, out var p) ? p.Value : Entity.Null;
                var local = LocalTransforms.TryGetComponent(origin, out var l) ? l : LocalTransform.Identity;
                var world = WorldTransforms.TryGetComponent(origin, out var w)
                    ? w
                    : new LocalToWorld { Value = local.ToMatrix() };
                var destinationWorld = WorldTransforms.TryGetComponent(destination, out var dw)
                    ? dw
                    : new LocalToWorld { Value = float4x4.identity };

                var hadPTM = PostTransformMatrices.HasComponent(origin);
                var ptm = hadPTM ? PostTransformMatrices[origin].Value : float4x4.identity;

                state = new EntityLinkAttachState
                {
                    ResolvedTarget = destination,
                    CapturedPreviousParent = originParent,
                    CapturedOriginalTransform = local,
                    CapturedOriginalPTM = ptm,
                    HadPostTransformMatrix = hadPTM,
                    IsAttached = true
                };

                ApplyTopology(chunk, origin, destination, local, ptm, hadPTM, world, destinationWorld,
                    config.TransformFlags);
            }

            private void ApplyTopology(int chunk, Entity origin, Entity destination, LocalTransform local, float4x4 ptm,
                bool hadPtm, LocalToWorld world, LocalToWorld destinationWorld, AttachmentTransformFlags flags)
            {
                if (flags.HasAny(AttachmentTransformFlags.SetParent))
                {
                    if (Parents.HasComponent(origin))
                        Commands.SetComponent(chunk, origin, new Parent { Value = destination });
                    else
                        Commands.AddComponent(chunk, origin, new Parent { Value = destination });
                }

                var resolvedTopology = Topology.Evaluate(local, ptm, hadPtm, world, destinationWorld, flags);

                Commands.SetComponent(chunk, origin, resolvedTopology.Local);

                if (resolvedTopology.HasPostTransform)
                {
                    if (hadPtm)
                        Commands.SetComponent(chunk, origin,
                            new PostTransformMatrix { Value = resolvedTopology.PostTransform });
                    else
                        Commands.AddComponent(chunk, origin,
                            new PostTransformMatrix { Value = resolvedTopology.PostTransform });
                }
                else if (hadPtm)
                {
                    Commands.RemoveComponent<PostTransformMatrix>(chunk, origin);
                }
            }
        }

        [BurstCompile]
        [WithNone(typeof(ClipActive))]
        [WithAll(typeof(ClipActivePrevious))]
        private partial struct DisconnectTransition : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Commands;
            [ReadOnly] public ComponentLookup<Parent> Parents;
            [ReadOnly] public ComponentLookup<PostTransformMatrix> PostTransformMatrices;

            private void Execute([ChunkIndexInQuery] int chunk, ref EntityLinkAttachState state,
                in TrackBinding binding)
            {
                if (!state.IsAttached) return;

                var origin = binding.Value;
                Commands.SetComponent(chunk, origin, state.CapturedOriginalTransform);

                var currentlyHasPtm = PostTransformMatrices.HasComponent(origin);

                if (state.HadPostTransformMatrix)
                {
                    if (currentlyHasPtm)
                        Commands.SetComponent(chunk, origin,
                            new PostTransformMatrix { Value = state.CapturedOriginalPTM });
                    else
                        Commands.AddComponent(chunk, origin,
                            new PostTransformMatrix { Value = state.CapturedOriginalPTM });
                }
                else if (currentlyHasPtm)
                {
                    Commands.RemoveComponent<PostTransformMatrix>(chunk, origin);
                }

                RevertTopology(chunk, origin, state.CapturedPreviousParent);
                state = default;
            }

            private void RevertTopology(int chunk, Entity origin, Entity previousParent)
            {
                if (previousParent == Entity.Null)
                {
                    Commands.RemoveComponent<Parent>(chunk, origin);
                    Commands.RemoveComponent<PreviousParent>(chunk, origin);
                }
                else
                {
                    if (Parents.HasComponent(origin))
                        Commands.SetComponent(chunk, origin, new Parent { Value = previousParent });
                    else
                        Commands.AddComponent(chunk, origin, new Parent { Value = previousParent });
                }
            }
        }
    }
}