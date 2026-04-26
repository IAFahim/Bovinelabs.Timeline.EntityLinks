using BovineLabs.Core.Authoring;
using UnityEngine;
using UnityEngine.Serialization;

namespace BovineLabs.Timeline.EntityLinks.Authoring
{
    public class SourceAuthoring : MonoBehaviour
    {
        [FormerlySerializedAs("entityLinkTagSchema")] public SourceSchema sourceSchema;

        private void OnValidate()
        {
            if (!transform.gameObject.TryGetComponent(out TransformAuthoring transformAuthoring))
                transform.gameObject.AddComponent<TransformAuthoring>();
        }
    }
}