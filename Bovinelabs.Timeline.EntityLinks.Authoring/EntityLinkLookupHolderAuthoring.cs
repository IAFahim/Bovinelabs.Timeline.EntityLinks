using System;
using System.Linq;
using BovineLabs.Timeline.EntityLinks.Data;
using Unity.Entities;
using UnityEngine;

namespace Bovinelabs.Timeline.EntityLinks.Authoring
{
    public class EntityLinkLookupHolderAuthoring : MonoBehaviour
    {
        public EntityTagAuthoring[] links = Array.Empty<EntityTagAuthoring>();

        private void OnValidate()
        {
            links = GetComponentsInChildren<EntityTagAuthoring>(true)
                .Where(entityTagAuthoring => entityTagAuthoring.GetComponentInParent<EntityLinkLookupHolderAuthoring>() == this)
                .ToArray();
        }

        public class EntityLinkLookupHolderBaker : Baker<EntityLinkLookupHolderAuthoring>
        {
            public override void Bake(EntityLinkLookupHolderAuthoring authoring)
            {
                var buffer = AddBuffer<EntityLookupStoreData>(GetEntity(TransformUsageFlags.None));
                foreach (var entityTagAuthoring in authoring.links)
                {
                    var entityLinkTagSchema = entityTagAuthoring.entityLinkTagSchema;
                    if (entityLinkTagSchema == null)
                    {
                        Debug.LogError(entityTagAuthoring.name, entityTagAuthoring);
                        continue;
                    }

                    buffer.Add(new EntityLookupStoreData
                    {
                        Tag = EntityLinkSettings.GetIndex(entityLinkTagSchema),
                        Value = GetEntity(entityTagAuthoring, TransformUsageFlags.None)
                    });
                }
            }
        }
    }
}