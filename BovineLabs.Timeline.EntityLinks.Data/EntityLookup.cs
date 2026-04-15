using Unity.Entities;

namespace BovineLabs.Timeline.EntityLinks.Data
{
    public struct EntityLookup
    {
        public byte Tag;
        public Entity Value;
    }

    public struct EntityLookupRegistry
    {
        public BlobArray<EntityLookup> Entries; // id sorted. So we can run binary search
    }

    public struct EntityLookupBlobComponent : IComponentData
    {
        public BlobAssetReference<EntityLookupRegistry> Blob;
    }

    public struct EntityLinkAttachRequestKey : IComponentData
    {
        public byte LinkKey;
    }

    public struct EntityLinkAttachRequestEntity : IComponentData
    {
        public Entity Entity;
    }

    public struct SetEntityLink : IComponentData, IEnableableComponent
    {
    }

    public struct SetEntityLinkPrevious : IComponentData, IEnableableComponent
    {
    }
}