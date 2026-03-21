using _Project._Code.Infrastructure.StaticData.AI;
using _Project._Code.Infrastructure.StaticData.Units;
using _Project._Code.Locale;
using UnityEngine;

namespace _Project._Code.Infrastructure.StaticData._Root
{
    [CreateAssetMenu(fileName = nameof(StaticDataService), menuName = "_Project/Infrastructure/New StaticDataService")]
    public sealed class StaticDataService : ScriptableObject
    {
        public MemoryPoolPipeline MemoryPoolPipeline;
        public UnitsStaticData UnitsStaticData;
        public BehaviourTreeStaticData BehaviourTreeStaticData;
    }
}