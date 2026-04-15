using BovineLabs.Essence.Authoring;
using BovineLabs.Timeline.EntityLinks.Data;
using Unity.Entities;
using UnityEngine;

namespace BovineLabs.Timeline.EntityLinks.Authoring
{
    public class EssenceRefComponentAuthoring : MonoBehaviour
    {
        public StatAuthoring statAuthoring;

        public class EntityEssenceRefComponentBaker : Baker<EssenceRefComponentAuthoring>
        {
            public override void Bake(EssenceRefComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,
                    new EntityEssenceRefComponent
                    {
                        EssenceEntity = GetEntity(authoring.statAuthoring.gameObject, TransformUsageFlags.None)
                    }
                );
            }
        }
    }
}