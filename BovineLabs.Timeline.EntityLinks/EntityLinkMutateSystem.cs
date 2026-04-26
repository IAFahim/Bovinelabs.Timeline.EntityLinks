using BovineLabs.Core.Collections;
using BovineLabs.Core.Extensions;
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
                Links = state.GetUnsafeBufferLookup<EntityLink>()
            }.ScheduleParallel(state.Dependency);
        }

        /// <summary>
        ///     Fires once per clip activation.
        ///     WithAll(ClipActive) + WithDisabled(ClipActivePrevious) = "just became active this frame".
        /// </summary>
        [BurstCompile]
        [WithAll(typeof(ClipActive))]
        [WithDisabled(typeof(ClipActivePrevious))]
        private partial struct MutateJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<Targets> TargetsLookup;
            [ReadOnly] public ComponentLookup<TargetsCustom> TargetsCustoms;
            [ReadOnly] public ComponentLookup<EntityLinkSource> Sources;

            [NativeDisableParallelForRestriction] public UnsafeBufferLookup<EntityLink> Links;

            private void Execute(Entity clipEntity, in TrackBinding binding, in EntityLinkMutate mutate)
            {
                var bindingEntity = binding.Value;
                if (bindingEntity == Entity.Null)
                    return;

                if (!TargetsLookup.TryGetComponent(bindingEntity, out var targets))
                    return;

                // Resolve the root entity that owns the link buffer
                var rootCandidate = targets.Get(mutate.ReadRootFrom, bindingEntity, TargetsCustoms);
                if (rootCandidate == Entity.Null)
                    return;

                if (!EntityLinkResolver.TryResolveRoot(rootCandidate, Sources, out var root))
                    return;

                if (!Links.TryGetBuffer(root, out var buffer))
                    return;

                switch (mutate.Mode)
                {
                    case EntityLinkMutateMode.Assign:
                        Assign(buffer, mutate.LinkKey, targets, mutate.NewTarget, bindingEntity);
                        break;

                    case EntityLinkMutateMode.Swap:
                        Swap(buffer, mutate.LinkKey, mutate.SwapKey);
                        break;

                    case EntityLinkMutateMode.Remove:
                        Remove(buffer, mutate.LinkKey);
                        break;
                }
            }

            /// <summary>
            ///     Assign a new target entity to the given key.
            ///     If the key exists, replace its target. Otherwise insert sorted.
            /// </summary>
            private void Assign(UnsafeDynamicBuffer<EntityLink> buffer, ushort key, in Targets targets,
                Target newTargetMode, Entity bindingEntity)
            {
                var newEntity = targets.Get(newTargetMode, bindingEntity, TargetsCustoms);

                for (var i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i].Key == key)
                    {
                        // Replace existing entry
                        var link = buffer[i];
                        link.Target = newEntity;
                        buffer[i] = link;
                        return;
                    }

                    if (buffer[i].Key > key)
                    {
                        // Insert in sorted order
                        buffer.Insert(i, new EntityLink { Key = key, Target = newEntity });
                        return;
                    }
                }

                // Append at end
                buffer.Add(new EntityLink { Key = key, Target = newEntity });
            }

            /// <summary>
            ///     Swap the targets of two link keys within the same buffer.
            ///     Used for inventory slot swaps, hand ↔ backpack, etc.
            /// </summary>
            private void Swap(UnsafeDynamicBuffer<EntityLink> buffer, ushort keyA, ushort keyB)
            {
                var indexA = -1;
                var indexB = -1;

                for (var i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i].Key == keyA) indexA = i;
                    if (buffer[i].Key == keyB) indexB = i;
                    if (indexA >= 0 && indexB >= 0) break;
                }

                if (indexA < 0 || indexB < 0)
                    return;

                var linkA = buffer[indexA];
                var linkB = buffer[indexB];
                buffer[indexA] = new EntityLink { Key = linkA.Key, Target = linkB.Target };
                buffer[indexB] = new EntityLink { Key = linkB.Key, Target = linkA.Target };
            }

            /// <summary>
            ///     Remove a link entry by key.
            /// </summary>
            private void Remove(UnsafeDynamicBuffer<EntityLink> buffer, ushort key)
            {
                for (var i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i].Key == key)
                    {
                        buffer.RemoveAt(i);
                        return;
                    }

                    if (buffer[i].Key > key)
                        return; // sorted, no match possible
                }
            }
        }
    }
}