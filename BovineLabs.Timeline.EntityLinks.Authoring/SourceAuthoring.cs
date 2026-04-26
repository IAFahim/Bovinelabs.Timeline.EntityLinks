using BovineLabs.Timeline.EntityLinks.Data;
using Unity.Entities;
using UnityEngine;

namespace BovineLabs.Timeline.EntityLinks.Authoring
{
    public class SourceAuthoring : MonoBehaviour
    {
        public SourceSchema sourceSchema;

        public class Baker : Baker<SourceAuthoring>
        {
            public override void Bake(SourceAuthoring authoring)
            {
                if (authoring.sourceSchema == null) return;
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity,
                    new SourceData
                    {
                        Root = GetEntity(authoring.transform.root, TransformUsageFlags.None)
                    });
            }
        }
    }
}