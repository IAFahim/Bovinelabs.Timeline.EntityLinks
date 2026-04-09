/* com.bovinelabs.timeline.entity.links/Runtime/EntityLinkAttachSystem.cs */
using BovineLabs.EntityLinks;
using BovineLabs.Timeline.Data;
using Unity.Burst;
using Unity.Entities;

namespace BovineLabs.Timeline.EntityLinks
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct EntityLinkAttachSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            state.Dependency = new EntityLinkAttachJob
            {
                ECB = ecb
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(ClipActive), typeof(OnClipActiveEntityLinkAttachTag))]
        [WithNone(typeof(ClipActivePrevious))]
        private partial struct EntityLinkAttachJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;

            private void Execute([ChunkIndexInQuery] int chunkIndex, in TrackBinding binding, in EntityLinkAttachConfig config)
            {
                var target = binding.Value;

                ECB.AddBuffer<EntityLookupRequestBuffer>(chunkIndex, target);
                ECB.AppendToBuffer(chunkIndex, target, new EntityLookupRequestBuffer
                {
                    Key = config.Key,
                    ResolveRule = config.ResolveRule
                });

                ECB.AddComponent<EntityLookupRequestedThisFrame>(chunkIndex, target);
                ECB.SetComponentEnabled<EntityLookupRequestedThisFrame>(chunkIndex, target, true);
                
                ECB.AddBuffer<EntityLookupResolveResult>(chunkIndex, target);
            }
        }
    }
}