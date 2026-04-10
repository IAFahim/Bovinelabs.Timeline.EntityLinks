using UnityEngine;

namespace Bovinelabs.Timeline.EntityLinks.Authoring
{
    [CreateAssetMenu(menuName = "BovineLabs/EntityLinks/Tag")]
    public class EntityLinkTagSchema : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Unique byte identifier for this link type. Must be unique across all EntityLinkTagSchema.")]
        private byte id;

        public byte Id => id;
    }
}