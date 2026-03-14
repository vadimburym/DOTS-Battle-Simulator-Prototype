using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace _Project._Code.Gameplay.CoreFeatures
{
    public struct EyeSensorGridSingleton : IComponentData
    {
        public NativeParallelMultiHashMap<int2, Entity> Command0Grid;
        public NativeParallelMultiHashMap<int2, Entity> Command1Grid;
        public float CellSize;
    }
}