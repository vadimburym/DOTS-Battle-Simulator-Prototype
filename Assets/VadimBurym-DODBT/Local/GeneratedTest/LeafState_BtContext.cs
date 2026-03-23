using _Project._Code.Gameplay.CoreFeatures.AI._Root;
using Unity.Collections;
using Unity.Entities;

namespace VadimBurym.DodBehaviourTree.Generated
{
    public ref struct LeafState_BtContext
    {
        public Entity Agent;
        [ReadOnly] public LeafData Data;
        public LeafStateElement State;
        [ReadOnly] public BtContext Context;
        public int SortKey;
    }
}