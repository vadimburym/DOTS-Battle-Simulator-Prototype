using UnityEngine;

namespace _Project._Code.Locale
{
    [CreateAssetMenu(fileName = nameof(MemoryPoolPipeline), menuName ="_Project/Infrastructure/New MemoryPoolPipeline")]
    public sealed class MemoryPoolPipeline : ScriptableObject
    {
        public GameObjectMemoryPoolData[] GameObjectMemoryPools;
    }
}