/* com.bovinelabs.timeline.entity.links/Authoring/EntityLinkAttachClip.cs */
using BovineLabs.Core.Keys;
using BovineLabs.EntityLinks;
using BovineLabs.Timeline.Authoring;
using BovineLabs.Timeline.Instantiate;
using Unity.Entities;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.EntityLinks.Authoring
{
    public sealed class EntityLinkAttachClip : DOTSClip, ITimelineClipAsset
    {
        [K(nameof(EntityLinkKeys))] public byte linkKey;
        public ResolveRule resolveRule = ResolveRule.Parent | ResolveRule.Owner;
        public ParentTransformConfig transformConfig = ParentTransformConfig.SetParent | ParentTransformConfig.SetTransform;

        public override double duration => 1;
        public ClipCaps clipCaps => ClipCaps.None;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            context.Baker.AddComponent(clipEntity, new EntityLinkAttachConfig
            {
                Key = linkKey,
                ResolveRule = resolveRule,
                TransformConfig = transformConfig
            });
            context.Baker.AddComponent(clipEntity, new OnClipActiveEntityLinkAttachTag());
            
            base.Bake(clipEntity, context);
        }
    }
}