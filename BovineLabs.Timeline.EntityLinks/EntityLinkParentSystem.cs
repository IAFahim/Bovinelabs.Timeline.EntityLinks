using BovineLabs.Reaction.Data.Core;
using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.EntityLinks.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace BovineLabs.Timeline.EntityLinks
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct EntityLinkParentSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EntityLinkParentData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            // Rising edge — attach weapon to resolved bone
            state.Dependency = new EnterJob
            {
                TargetsLookup = SystemAPI.GetComponentLookup<Targets>(true),
                TargetsCustoms = SystemAPI.GetComponentLookup<TargetsCustom>(true),
                Sources = SystemAPI.GetComponentLookup<EntityLinkSource>(true),
                Links = SystemAPI.GetBufferLookup<EntityLink>(true),
                ParentLookup = SystemAPI.GetComponentLookup<Parent>(true),
                ECB = ecb
            }.ScheduleParallel(state.Dependency);

            // Falling edge — restore previous parent
            state.Dependency = new ExitJob
            {
                ECB = ecb
            }.ScheduleParallel(state.Dependency);
        }

        /// <summary>
        ///     Fires once when clip becomes active (ClipActive && !ClipActivePrevious).
        ///     Resolves the bone via EntityLink, records previous parent, reparents.
        /// </summary>
        [BurstCompile]
        [WithAll(typeof(ClipActive))]
        [WithDisabled(typeof(ClipActivePrevious))]
        private partial struct EnterJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<Targets> TargetsLookup;
            [ReadOnly] public ComponentLookup<TargetsCustom> TargetsCustoms;
            [ReadOnly] public ComponentLookup<EntityLinkSource> Sources;
            [ReadOnly] public BufferLookup<EntityLink> Links;
            [ReadOnly] public ComponentLookup<Parent> ParentLookup;

            public EntityCommandBuffer.ParallelWriter ECB;

            private void Execute(
                Entity clipEntity,
                [EntityIndexInQuery] int sortKey,
                in TrackBinding binding,
                in EntityLinkParentData config,
                ref EntityLinkParentState state)
            {
                var bindingEntity = binding.Value;
                if (bindingEntity == Entity.Null)
                    return;

                if (!TargetsLookup.TryGetComponent(bindingEntity, out var targets))
                    return;

                // 1. Resolve the entity to reparent (e.g. the weapon)
                var entityToParent = targets.Get(config.EntityToParent, bindingEntity, TargetsCustoms);
                if (entityToParent == Entity.Null)
                    return;

                // 2. Resolve the link target bone (e.g. Hand_R)
                var rootCandidate = targets.Get(config.ReadRootFrom, bindingEntity, TargetsCustoms);
                if (rootCandidate == Entity.Null)
                    return;

                if (!EntityLinkResolver.TryResolveRoot(rootCandidate, Sources, out var root))
                    return;

                if (!EntityLinkResolver.TryResolveFromRoot(root, config.ParentLinkKey, Links, out var resolvedParent))
                    return;

                // 3. Record previous state for restoration on clip end
                state.Target = entityToParent;
                state.HadParent = ParentLookup.TryGetComponent(entityToParent, out var oldParent);
                state.PreviousParent = state.HadParent ? oldParent.Value : Entity.Null;

                // 4. Reparent to the resolved bone
                ECB.AddComponent(sortKey, entityToParent, new Parent { Value = resolvedParent });
                ECB.SetComponent(sortKey, entityToParent,
                    LocalTransform.FromPositionRotation(config.LocalPosition, config.LocalRotation));
            }
        }

        /// <summary>
        ///     Fires once when clip stops being active (!ClipActive && ClipActivePrevious).
        ///     Restores the previous parent if configured.
        /// </summary>
        [BurstCompile]
        [WithAll(typeof(ClipActivePrevious))]
        [WithDisabled(typeof(ClipActive))]
        private partial struct ExitJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;

            private void Execute(
                [EntityIndexInQuery] int sortKey,
                in EntityLinkParentData config,
                in EntityLinkParentState state)
            {
                if (!config.RestoreOnEnd || state.Target == Entity.Null)
                    return;

                if (state.HadParent && state.PreviousParent != Entity.Null)
                    ECB.AddComponent(sortKey, state.Target, new Parent { Value = state.PreviousParent });
                else
                    ECB.RemoveComponent<Parent>(sortKey, state.Target);
            }
        }
    }
}