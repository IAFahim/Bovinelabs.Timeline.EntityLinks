using BovineLabs.Core.Iterators;
using Unity.Entities;

namespace BovineLabs.Timeline.EntityLinks.Data
{
    public struct EntityLink : IDynamicHashMap<ushort, Entity>
    {
        private byte value;
        public byte Value => this.value;
    }
}