/* com.bovinelabs.timeline.entity.links/Runtime/Data.cs */
using BovineLabs.EntityLinks;
using BovineLabs.Timeline.Instantiate;
using Unity.Entities;

namespace BovineLabs.Timeline.EntityLinks
{
    public struct EntityLinkAttachConfig : IComponentData
    {
        public byte Key;
        public ResolveRule ResolveRule;
        public ParentTransformConfig TransformConfig;
    }

    public struct EntityLinkInstantiateConfig : IComponentData
    {
        public Entity Prefab;
        public byte Key;
        public ResolveRule ResolveRule;
        public ParentTransformConfig TransformConfig;
    }

    public struct OnClipActiveEntityLinkAttachTag : IComponentData { }
    
    public struct OnClipActiveEntityLinkInstantiateTag : IComponentData { }
}