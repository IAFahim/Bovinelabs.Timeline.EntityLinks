using BovineLabs.Core.Iterators;
using BovineLabs.Reaction.Data.Core;
using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.EntityLinks.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace BovineLabs.Timeline.EntityLinks
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct EntityLinkMutateSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EntityLinkMutate>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new MutateJob
            {
                TargetsLookup = SystemAPI.GetComponentLookup<Targets>(true),
                TargetsCustoms = SystemAPI.GetComponentLookup<TargetsCustom>(true),
                Sources = SystemAPI.GetComponentLookup<EntityLinkSource>(true),
                Links = SystemAPI.GetBufferLookup<EntityLink>(false)
            }.Schedule(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(ClipActive))]
        [WithDisabled(typeof(ClipActivePrevious))]
        private partial struct MutateJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<Targets> TargetsLookup;
            [ReadOnly] public ComponentLookup<TargetsCustom> TargetsCustoms;
            [ReadOnly] public ComponentLookup<EntityLinkSource> Sources;

            public BufferLookup<EntityLink> Links;

            private void Execute(Entity clipEntity, in TrackBinding binding, in EntityLinkMutate mutate)
            {
                var bindingEntity = binding.Value;
                if (bindingEntity == Entity.Null)
                    return;

                if (!TargetsLookup.TryGetComponent(bindingEntity, out var targets))
                    return;

                var rootCandidate = targets.Get(mutate.ReadRootFrom, bindingEntity, TargetsCustoms);
                if (rootCandidate == Entity.Null)
                    return;

                if (!EntityLinkResolver.TryResolveRoot(rootCandidate, Sources, out var root))
                    return;

                if (!Links.TryGetBuffer(root, out var buffer))
                    return;

                var map = buffer.AsHashMap<EntityLink, ushort, Entity>();
                
                switch (mutate.Mode)
                {
                    case EntityLinkMutateMode.Assign:
                        map[mutate.LinkKey] = targets.Get(mutate.NewTarget, bindingEntity, TargetsCustoms);
                        break;

                    case EntityLinkMutateMode.Swap:
                        var hasA = map.TryGetValue(mutate.LinkKey, out var targetA);
                        var hasB = map.TryGetValue(mutate.SwapKey, out var targetB);

                        if (hasA) map[mutate.SwapKey] = targetA; else map.Remove(mutate.SwapKey);
                        if (hasB) map[mutate.LinkKey] = targetB; else map.Remove(mutate.LinkKey);
                        break;

                    case EntityLinkMutateMode.Remove:
                        map.Remove(mutate.LinkKey);
                        break;
                }
            }
        }
    }
}