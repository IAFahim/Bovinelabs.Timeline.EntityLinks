/* com.bovinelabs.timeline.entity.links/Runtime/EntityLinkInstantiateSystem.cs */
using BovineLabs.EntityLinks;
using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.Instantiate;
using BovineLabs.Timeline.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace BovineLabs.Timeline.EntityLinks
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct EntityLinkInstantiateSystem : ISystem
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

            var localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true);

            state.Dependency = new EntityLinkInstantiateJob
            {
                ECB = ecb,
                LocalToWorldLookup = localToWorldLookup
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(ClipActive), typeof(OnClipActiveEntityLinkInstantiateTag))]
        [WithNone(typeof(ClipActivePrevious))]
        private partial struct EntityLinkInstantiateJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter ECB;
            [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldLookup;

            private void Execute([ChunkIndexInQuery] int chunkIndex, in TrackBinding binding, in EntityLinkInstantiateConfig config)
            {
                var instance = ECB.Instantiate(chunkIndex, config.Prefab);
                var target = binding.Value;

                if (config.TransformConfig.HasAny(ParentTransformConfig.SetTransform))
                {
                    if (LocalToWorldLookup.TryGetComponent(target, out var targetLtw))
                    {
                        targetLtw.Value.ExtractLocalTransform(out var localTransform);
                        ECB.SetComponent(chunkIndex, instance, localTransform);
                    }
                }

                ECB.AddBuffer<EntityLookupRequestBuffer>(chunkIndex, instance);
                ECB.AppendToBuffer(chunkIndex, instance, new EntityLookupRequestBuffer
                {
                    Key = config.Key,
                    ResolveRule = config.ResolveRule
                });

                ECB.AddComponent<EntityLookupRequestedThisFrame>(chunkIndex, instance);
                ECB.SetComponentEnabled<EntityLookupRequestedThisFrame>(chunkIndex, instance, true);
                
                ECB.AddBuffer<EntityLookupResolveResult>(chunkIndex, instance);
            }
        }
    }
}