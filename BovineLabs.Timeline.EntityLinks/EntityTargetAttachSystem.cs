using BovineLabs.Core.Extensions;
using BovineLabs.Core.Iterators;
using BovineLabs.Reaction.Data.Core;
using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.EntityLinks.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace BovineLabs.Timeline.EntityLinks.Systems
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct EntityTargetAttachSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            state.Dependency = new ApplyEntityEssenceJob
            {
                TargetsLookup = state.GetUnsafeComponentLookup<Targets>(true),
                EssenceRefLookup = SystemAPI.GetComponentLookup<EntityEssenceRef>(true),
                ECB = ecb
            }.Schedule(state.Dependency);

            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [BurstCompile]
    [WithAll(typeof(ClipActive), typeof(EntityEssenceClipTag))]
    [WithDisabled(typeof(ClipActivePrevious))]
    internal partial struct ApplyEntityEssenceJob : IJobEntity
    {
        [ReadOnly] public UnsafeComponentLookup<Targets> TargetsLookup;
        [ReadOnly] public ComponentLookup<EntityEssenceRef> EssenceRefLookup;
        public EntityCommandBuffer ECB;

        private void Execute(in TrackBinding binding)
        {
            var bindingEntity = binding.Value;
            if (!EssenceRefLookup.TryGetComponent(bindingEntity, out var essenceRef)) return;
            if (essenceRef.Value == Entity.Null) return;

            if (!TargetsLookup.HasComponent(bindingEntity)) return;
            var targets = TargetsLookup[bindingEntity];
            targets.Target = essenceRef.Value;
            ECB.SetComponent(bindingEntity, targets);
        }
    }
}