/* com.bovinelabs.timeline.entity.links/Authoring/EntityLinkAttachTrack.cs */
using System;
using System.ComponentModel;
using BovineLabs.Timeline.Authoring;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.EntityLinks.Authoring
{
    [Serializable]
    [TrackClipType(typeof(EntityLinkAttachClip))]
    [TrackClipType(typeof(EntityLinkInstantiateClip))]
    [TrackColor(0.2f, 0.8f, 0.4f)]
    [TrackBindingType(typeof(GameObject))]
    [DisplayName("BovineLabs/Timeline/Entity Links/Attach Track")]
    public sealed class EntityLinkAttachTrack : DOTSTrack
    {
    }
}