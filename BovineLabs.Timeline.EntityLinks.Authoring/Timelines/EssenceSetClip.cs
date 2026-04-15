using BovineLabs.Timeline.Authoring;
using BovineLabs.Timeline.EntityLinks.Data;
using Unity.Entities;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.EntityLinks.Authoring.Timelines
{
    public class EssenceSetClip : DOTSClip, ITimelineClipAsset
    {
        public override double duration => 1;
        public ClipCaps clipCaps => ClipCaps.None;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            context.Baker.AddComponent(clipEntity, new EntityEssenceClipTag());
            base.Bake(clipEntity, context);
        }
    }
}