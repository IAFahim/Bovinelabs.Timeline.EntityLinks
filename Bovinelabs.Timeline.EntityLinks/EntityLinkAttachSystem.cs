using BovineLabs.Core.Extensions;
using BovineLabs.Core.Iterators;
using BovineLabs.Reaction.Data.Core;
using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.EntityLinks.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace BovineLabs.Timeline.EntityLinks
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct EntityLinkAttachSystem : ISystem
    {
        private UnsafeComponentLookup<Parent> parents;
        private UnsafeComponentLookup<Targets> targets;
        private UnsafeBufferLookup<EntityLookupStoreData> stores;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.parents = state.GetUnsafeComponentLookup<Parent>(true);
            this.targets = state.GetUnsafeComponentLookup<Targets>(true);
            this.stores = state.GetUnsafeBufferLookup<EntityLookupStoreData>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.parents.Update(ref state);
            this.targets.Update(ref state);
            this.stores.Update(ref state);

            var commands = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var graph = new Graph
            {
                Parents = this.parents,
                Targets = this.targets,
                Stores = this.stores
            };

            state.Dependency = new ConnectTransition
            {
                Commands = commands,
                Graph = graph,
                AccentLimit = 1,
                Parents = this.parents,
            }.ScheduleParallel(state.Dependency);

            state.Dependency = new DisconnectTransition
            {
                Commands = commands,
                Parents = this.parents,
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(ClipActive))]
        [WithNone(typeof(ClipActivePrevious))]
        private partial struct ConnectTransition : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Commands;
            public Graph Graph;
            [ReadOnly] public byte AccentLimit;
            [ReadOnly] public UnsafeComponentLookup<Parent> Parents;

            private void Execute([ChunkIndexInQuery] int chunk, ref EntityLinkAttachState state,
                in EntityLinkAttachConfig config, in TrackBinding binding)
            {
                var origin = binding.Value;
                this.Graph.Evaluate(origin, config.LinkKey, config.ResolveRule, this.AccentLimit, out var destination);

                if (destination == Entity.Null)
                {
                    state = default;
                    return;
                }

                var originParent = this.Parents.TryGetComponent(origin, out var p) ? p.Value : Entity.Null;

                state = new EntityLinkAttachState
                {
                    ResolvedTarget = destination,
                    CapturedPreviousParent = originParent,
                    IsAttached = true
                };

            }
        }


        [BurstCompile]
        [WithNone(typeof(ClipActive))]
        [WithAll(typeof(ClipActivePrevious))]
        private partial struct DisconnectTransition : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter Commands;
            [ReadOnly] public UnsafeComponentLookup<Parent> Parents;

            private void Execute([ChunkIndexInQuery] int chunk, ref EntityLinkAttachState state, in TrackBinding binding)
            {
                if (!state.IsAttached) return;

                var origin = binding.Value;
                
                this.RevertTopology(chunk, origin, state.CapturedPreviousParent);
                state = default;
            }

            private void RevertTopology(int chunk, Entity origin, Entity previousParent)
            {
                if (previousParent == Entity.Null)
                {
                    this.Commands.RemoveComponent<Parent>(chunk, origin);
                    this.Commands.RemoveComponent<PreviousParent>(chunk, origin);
                }
                else
                {
                    if (this.Parents.TryGetComponent(origin, out _))
                        this.Commands.SetComponent(chunk, origin, new Parent { Value = previousParent });
                    else
                        this.Commands.AddComponent(chunk, origin, new Parent { Value = previousParent });
                }
            }
        }
    }
}