using System;
using System.ComponentModel;
using BovineLabs.Reaction.Authoring.Core;
using BovineLabs.Timeline.Authoring;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.EntityLinks.Authoring
{
    [Serializable]
    [TrackClipType(typeof(EntityLinkParentClip))]
    [TrackColor(0.8f, 0.2f, 0.8f)]
    [TrackBindingType(typeof(TargetsAuthoring))]
    [DisplayName("BovineLabs/Entity Links/Parent")]
    public sealed class EntityLinkParentTrack : DOTSTrack
    {
    }
}