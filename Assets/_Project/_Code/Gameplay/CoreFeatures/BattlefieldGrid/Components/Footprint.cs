using Unity.Entities;

namespace _Project._Code.Gameplay.CoreFeatures
{
    public struct Footprint : IComponentData
    {
        public byte FootprintX;
        public byte FootprintY;
    }
}