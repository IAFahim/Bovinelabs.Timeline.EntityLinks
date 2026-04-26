using BovineLabs.Core.ObjectManagement;
using BovineLabs.Core.PropertyDrawers;
using UnityEngine;

namespace BovineLabs.Timeline.EntityLinks.Authoring
{
    [AutoRef(nameof(EntityLinkSettings), "schemas", nameof(EntityLinkSchema), "Schemas/EntityLinks/")]
    [CreateAssetMenu(menuName = "BovineLabs/Entity Links/Schema")]
    public sealed class EntityLinkSchema : ScriptableObject, IUID
    {
        [SerializeField] [InspectorReadOnly] private ushort id;

        public ushort Id => id;

        int IUID.ID
        {
            get => id;
            set
            {
                if (value is < 0 or > ushort.MaxValue)
                {
                    Debug.LogError("Ran out of EntityLink schema keys.");
                    return;
                }

                id = (ushort)value;
            }
        }

        public static implicit operator ushort(EntityLinkSchema schema)
        {
            return schema == null ? (ushort)0 : schema.id;
        }
    }
}