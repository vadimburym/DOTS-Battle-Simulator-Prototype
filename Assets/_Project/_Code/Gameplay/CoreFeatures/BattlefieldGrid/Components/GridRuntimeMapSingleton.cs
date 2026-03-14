using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace _Project._Code.Gameplay.CoreFeatures
{
    public struct GridRuntimeMapSingleton : IComponentData
    {
        public NativeParallelHashMap<int2, Entity> OccupiedMap;
    }
}