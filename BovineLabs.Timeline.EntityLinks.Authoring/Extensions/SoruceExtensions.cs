using BovineLabs.Timeline.Authoring;
using UnityEngine;

namespace BovineLabs.Timeline.EntityLinks.Authoring.Extensions
{
    public static class SoruceExtensions
    {
        public static bool TryResolveLink(this BakingContext context, SourceSchema schema, out GameObject linkedGo)
        {
            linkedGo = null;
            if (schema == null) return false;

            var binding = context.Director.GetGenericBinding(context.Track);
            var targetGo = binding as GameObject ?? (binding as Component)?.gameObject;
            
            if (targetGo == null) return false;

            var registry = targetGo.transform.root.GetComponent<RootSourceRegistryAuthoring>();
            if (registry == null) return false;

            foreach (var tag in registry.entityTagAuthorings)
            {
                if (tag != null && tag.sourceSchema == schema)
                {
                    linkedGo = tag.gameObject;
                    return true;
                }
            }

            return false;
        }
    }
}