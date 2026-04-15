using Unity.Entities;

namespace BovineLabs.Timeline.EntityLinks.Data
{
    public struct EntityEssenceRefComponent : IComponentData
    {
        public Entity EssenceEntity;
    }
}