using Unity.Entities;
using Unity.Mathematics;

namespace _Project._Code.Gameplay.CoreFeatures
{
    public struct GridNavigationState : IComponentData
    {
        public int2 MovingCell;
        public int2 OccupiedCell;
        public byte HasOccupiedCell;
    }
}