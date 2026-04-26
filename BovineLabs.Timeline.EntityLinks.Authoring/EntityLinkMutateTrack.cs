using System;
using System.ComponentModel;
using BovineLabs.Reaction.Authoring.Core;
using BovineLabs.Timeline.Authoring;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.EntityLinks.Authoring
{
    [Serializable]
    [TrackClipType(typeof(EntityLinkMutateClip))]
    [TrackColor(0.85f, 0.55f, 0.2f)]
    [TrackBindingType(typeof(TargetsAuthoring))]
    [DisplayName("BovineLabs/Entity Links/Link Mutate")]
    public sealed class EntityLinkMutateTrack : DOTSTrack
    {
    }
}