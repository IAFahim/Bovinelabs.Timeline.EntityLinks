using BovineLabs.Reaction.Data.Core;
using BovineLabs.Timeline.Authoring;
using BovineLabs.Timeline.EntityLinks.Data;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.EntityLinks.Authoring
{
    public sealed class EntityLinkMutateClip : DOTSClip, ITimelineClipAsset
    {
        [Header("Operation")]
        public EntityLinkMutateMode mode = EntityLinkMutateMode.Assign;

        [Header("Link")]
        public EntityLinkSchema link;
        public Target readRootFrom = Target.Source;

        [Header("Assign / Swap Target")]
        public Target newTarget = Target.Target;

        [Header("Swap")]
        [Tooltip("Second link key for swap operations. The entity at this key is swapped with the entity at Link.")]
        public EntityLinkSchema swapLink;

        public override double duration => 1;
        public ClipCaps clipCaps => ClipCaps.None;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            if (!EntityLinkAuthoringUtility.TryGetKey(this.link, out ushort key))
            {
                Debug.LogError($"{nameof(EntityLinkMutateClip)} '{this.name}' missing link schema.");
                return;
            }

            EntityLinkAuthoringUtility.TryGetKey(this.swapLink, out ushort swapKey);

            context.Baker.AddComponent(clipEntity, new EntityLinkMutate
            {
                Mode = this.mode,
                ReadRootFrom = this.readRootFrom,
                LinkKey = key,
                NewTarget = this.newTarget,
                SwapKey = swapKey,
            });

            base.Bake(clipEntity, context);
        }
    }
}
