using BovineLabs.Core.Authoring;
using UnityEngine;

namespace BovineLabs.Timeline.EntityLinks.Authoring
{
    public class EntityTagAuthoring : MonoBehaviour
    {
        public EntityLinkTagSchema entityLinkTagSchema;

        private void OnValidate()
        {
            if (!transform.gameObject.TryGetComponent(out TransformAuthoring transformAuthoring))
                transform.gameObject.AddComponent<TransformAuthoring>();
        }
    }
}