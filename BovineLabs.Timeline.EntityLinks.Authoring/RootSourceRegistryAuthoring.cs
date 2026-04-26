using System;
using System.Collections.Generic;
using BovineLabs.Timeline.EntityLinks.Data;
using Unity.Entities;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BovineLabs.Timeline.EntityLinks.Authoring
{
    [DisallowMultipleComponent]
    public class RootSourceRegistryAuthoring : MonoBehaviour
    {
        public SourceAuthoring[] entityTagAuthorings = Array.Empty<SourceAuthoring>();
        public bool allowInactive = true;

        private void OnValidate()
        {
            if (transform.root != transform)
                Debug.LogWarning($"[EntityLinkRegistry] {name} must be root.", this);

            entityTagAuthorings = GetComponentsInChildren<SourceAuthoring>(allowInactive);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (entityTagAuthorings == null) return;
            
            foreach (var tag in entityTagAuthorings)
            {
                if (tag == null || tag.sourceSchema == null) continue;
                
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, tag.transform.position);
                
                var labelPos = tag.transform.position + Vector3.up * 0.5f;
                Handles.Label(labelPos, $"[Link] {tag.sourceSchema.name}");
            }
        }
#endif

        public class Baker : Baker<RootSourceRegistryAuthoring>
        {
            public override void Bake(RootSourceRegistryAuthoring authoring)
            {
                if (authoring.transform.root != authoring.transform) return;

                var validLinks = new List<(byte tag, SourceAuthoring src)>();
                var tagsSet = new HashSet<byte>();

                foreach (var tagAuth in authoring.entityTagAuthorings)
                {
                    if (tagAuth == null || tagAuth.sourceSchema == null) continue;

                    var schema = tagAuth.sourceSchema;

                    if (!tagsSet.Add(schema.Id))
                    {
                        Debug.LogError($"Duplicate schema tag '{schema.name}' on {tagAuth.name}", tagAuth.gameObject);
                        continue;
                    }

                    validLinks.Add((schema.Id, tagAuth));
                    DependsOn(tagAuth);
                }

                if (validLinks.Count == 0) return;

                var entity = GetEntity(TransformUsageFlags.None);
                var buffer = AddBuffer<EntityLinkElement>(entity);
                buffer.ResizeUninitialized(validLinks.Count);

                for (var i = 0; i < validLinks.Count; i++)
                {
                    buffer[i] = new EntityLinkElement
                    {
                        Tag = validLinks[i].tag,
                        Value = GetEntity(validLinks[i].src, TransformUsageFlags.None)
                    };
                }
            }
        }
    }
}