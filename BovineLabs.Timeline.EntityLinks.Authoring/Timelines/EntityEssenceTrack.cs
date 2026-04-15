using System;
using System.ComponentModel;
using BovineLabs.Reaction.Authoring.Core;
using BovineLabs.Timeline.Authoring;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.EntityLinks.Authoring.Timelines
{
    [Serializable]
    [TrackClipType(typeof(EssenceSetClip))]
    [TrackColor(0.2f, 0.8f, 0.8f)]
    [TrackBindingType(typeof(TargetsAuthoring))]
    [DisplayName("BovineLabs/Timeline/Entity Links/Essence Set")]
    public class EntityEssenceTrack : DOTSTrack
    {
    }
}