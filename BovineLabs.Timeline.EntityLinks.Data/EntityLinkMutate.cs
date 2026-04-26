using BovineLabs.Reaction.Data.Core;
using Unity.Entities;

namespace BovineLabs.Timeline.EntityLinks.Data
{
    public enum EntityLinkMutateMode : byte
    {
        Assign = 0,
        Swap = 1,
        Remove = 2
    }

    public struct EntityLinkMutate : IComponentData
    {
        public EntityLinkMutateMode Mode;
        public Target ReadRootFrom;
        public ushort LinkKey;

        /// <summary>Target entity to write into the link (Assign/Swap only).</summary>
        public Target NewTarget;

        /// <summary>Second key for swap operations (Swap only). 0 = swap with Entity.Null.</summary>
        public ushort SwapKey;
    }
}