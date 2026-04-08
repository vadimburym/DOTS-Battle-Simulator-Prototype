using _Project._Code.Gameplay.CoreFeatures.AI._Root;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace VadimBurym.DodBehaviourTree.Generated
{
    public ref struct RunnerState_BtContext
    {
        public Entity Agent;
        [ReadOnly] public LeafData LeafData;
        public LeafStateElement LeafState;
        [ReadOnly] public BtContext Context;
        public Random Random;
        public int SortKey;
    }
}
