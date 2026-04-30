using BovineLabs.Core.Iterators;
using BovineLabs.Core.ObjectManagement;
using Unity.Entities;

namespace BovineLabs.Timeline.EntityLinks.Data
{
    public struct EntityLink : IDynamicHashMap<ObjectId, Entity>
    {
        private byte value;

        public byte Value => this.value;
    }
}
