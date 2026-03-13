using Unity.Entities;

namespace _Project._Code.Gameplay.CoreFeatures
{
    public struct UnitBody : IComponentData
    {
        public byte Team;
        public byte FootprintX;
        public byte FootprintY;
    }
}