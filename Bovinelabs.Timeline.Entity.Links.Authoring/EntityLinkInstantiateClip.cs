/* com.bovinelabs.timeline.entity.links/Authoring/EntityLinkInstantiateClip.cs */
using BovineLabs.Core.Keys;
using BovineLabs.EntityLinks;
using BovineLabs.Timeline.Authoring;
using BovineLabs.Timeline.Instantiate;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.EntityLinks.Authoring
{
    public sealed class EntityLinkInstantiateClip : DOTSClip, ITimelineClipAsset
    {
        public GameObject prefab;
        [K(nameof(EntityLinkKeys))] public byte linkKey;
        public ResolveRule resolveRule = ResolveRule.Parent | ResolveRule.Owner;
        public ParentTransformConfig parentTransformConfig = ParentTransformConfig.SetParent | ParentTransformConfig.SetTransform;

        public override double duration => 1;
        public ClipCaps clipCaps => ClipCaps.None;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            context.Baker.AddComponent(clipEntity, new EntityLinkInstantiateConfig
            {
                Prefab = context.Baker.GetEntity(prefab, TransformUsageFlags.None),
                Key = linkKey,
                ResolveRule = resolveRule,
                TransformConfig = parentTransformConfig
            });
            context.Baker.AddComponent(clipEntity, new OnClipActiveEntityLinkInstantiateTag());
            
            base.Bake(clipEntity, context);
        }
    }
}