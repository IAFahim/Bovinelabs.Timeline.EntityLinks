using System;
using System.Collections.Generic;
using BovineLabs.Timeline.EntityLinks.Data;
using Unity.Entities;
using UnityEngine;

namespace BovineLabs.Timeline.EntityLinks.Authoring
{
    [DisallowMultipleComponent]
    public sealed class EntityLinkRootAuthoring : MonoBehaviour
    {
        public bool AutoCollectAnchors = true;
        public Link[] Links = Array.Empty<Link>();

        [Serializable]
        public sealed class Link
        {
            public EntityLinkSchema Schema;
            public EntityLinkSourceAuthoring Target;

            public bool TryGetTarget(out EntityLinkSourceAuthoring target)
            {
                target = Target;
                return target != null;
            }
        }

        private enum LinkOrigin : byte
        {
            Auto,
            Manual
        }

        private sealed class Baker : Baker<EntityLinkRootAuthoring>
        {
            public override void Bake(EntityLinkRootAuthoring authoring)
            {
                var rootEntity = GetEntity(TransformUsageFlags.None);
                var links = new Dictionary<ushort, EntityLinkAuthoringUtility.Entry>();

                if (authoring.AutoCollectAnchors) CollectSources(authoring, links);

                CollectManualLinks(authoring, links);

                var entries = new List<EntityLinkAuthoringUtility.Entry>(links.Values);
                entries.Sort((a, b) => a.Key.CompareTo(b.Key));


                var buffer = AddBuffer<EntityLink>(rootEntity);
                foreach (var entry in entries)
                    buffer.Add(new EntityLink
                    {
                        Key = entry.Key,
                        Target = GetEntity(entry.Target, TransformUsageFlags.None)
                    });
            }

            private void CollectSources(EntityLinkRootAuthoring root,
                Dictionary<ushort, EntityLinkAuthoringUtility.Entry> links)
            {
                var sources = root.GetComponentsInChildren<EntityLinkSourceAuthoring>(true);
                var schemas = new List<EntityLinkSchema>(4);
                var seenSchemas = new HashSet<EntityLinkSchema>();

                foreach (var source in sources)
                {
                    DependsOn(source);

                    if (!source.TryGetRoot(out var sourceRoot) || sourceRoot != root) continue;

                    schemas.Clear();
                    source.AddSchemas(schemas);

                    foreach (var schema in schemas)
                    {
                        if (!EntityLinkAuthoringUtility.TryGetKey(schema, out var key)) continue;

                        if (seenSchemas.Add(schema)) DependsOn(schema);
                        AddLink(root, links, key, source, schema.name, LinkOrigin.Auto);
                    }
                }
            }

            private void CollectManualLinks(EntityLinkRootAuthoring root,
                Dictionary<ushort, EntityLinkAuthoringUtility.Entry> links)
            {
                var seenSchemas = new HashSet<EntityLinkSchema>();

                foreach (var link in root.Links)
                {
                    if (link == null) continue;

                    if (!EntityLinkAuthoringUtility.TryGetKey(link.Schema, out var key)) continue;

                    if (!link.TryGetTarget(out var target))
                    {
                        Debug.LogError($"EntityLink '{link.Schema.name}' on '{root.name}' has null target.");
                        continue;
                    }

                    if (seenSchemas.Add(link.Schema)) DependsOn(link.Schema);
                    DependsOn(target);
                    AddLink(root, links, key, target, link.Schema.name, LinkOrigin.Manual);
                }
            }

            private void AddLink(
                EntityLinkRootAuthoring root,
                Dictionary<ushort, EntityLinkAuthoringUtility.Entry> links,
                ushort key,
                EntityLinkSourceAuthoring target,
                string name,
                LinkOrigin origin)
            {
                if (target == null)
                {
                    Debug.LogError($"EntityLink '{name}' on '{root.name}' has null target.");
                    return;
                }

                if (!target.TryGetRoot(out var targetRoot))
                {
                    Debug.LogError($"EntityLink '{name}' target '{target.name}' has no root.");
                    return;
                }

                if (targetRoot != root)
                {
                    Debug.LogError(
                        $"EntityLink '{name}' on '{root.name}' targets '{target.name}' under different root '{targetRoot.name}'.");
                    return;
                }

                var entry = new EntityLinkAuthoringUtility.Entry(key, target, origin == LinkOrigin.Manual);

                if (!links.TryGetValue(key, out var existing))
                {
                    links.Add(key, entry);
                    return;
                }

                if (!existing.IsManual && entry.IsManual)
                {
                    links[key] = entry;
                    return;
                }

                Debug.LogError($"Duplicate EntityLink '{name}' on '{root.name}'.");
            }
        }
    }
}