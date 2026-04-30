using BovineLabs.Core.Iterators;
using BovineLabs.Timeline.EntityLinks.Data;
using Unity.Collections;
using Unity.Entities;

namespace BovineLabs.Timeline.EntityLinks
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct EntityLinkBuildSystem : ISystem
    {
        private EntityQuery _query;

        public void OnCreate(ref SystemState state)
        {
            _query = SystemAPI.QueryBuilder()
                .WithAll<EntityLinkEntry, EntityLink>()
                .Build();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_query.IsEmpty) return;

            var entities = _query.ToEntityArray(Allocator.Temp);
            var entriesLookup = SystemAPI.GetBufferLookup<EntityLinkEntry>(true);
            var linksLookup = SystemAPI.GetBufferLookup<EntityLink>(false);
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                if (!entriesLookup.TryGetBuffer(entity, out var entries) || entries.Length == 0) continue;

                var links = linksLookup[entity];
                var map = links.AsHashMap<EntityLink, ushort, Entity>();

                for (int j = 0; j < entries.Length; j++)
                    map.Add(entries[j].Key, entries[j].Target);

                ecb.RemoveComponent<EntityLinkEntry>(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            entities.Dispose();
        }
    }
}
