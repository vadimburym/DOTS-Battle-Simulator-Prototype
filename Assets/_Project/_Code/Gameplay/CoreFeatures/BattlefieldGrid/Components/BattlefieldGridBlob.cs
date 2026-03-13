using Unity.Entities;
using Unity.Mathematics;

namespace _Project._Code.Gameplay.CoreFeatures
{
    public struct BattlefieldGridBlob
    {
        public float3 Origin;
        public float CellSize;
        public int Width;
        public int Height;
        public BlobArray<byte> Walkable;
    }
}