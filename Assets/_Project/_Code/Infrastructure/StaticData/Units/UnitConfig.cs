using _Project._Code.Core.Keys;
using UnityEngine;

namespace _Project._Code.Infrastructure.StaticData.Units
{
    [CreateAssetMenu(fileName = nameof(UnitConfig), menuName = "_Project/Core/New UnitConfig")]
    public sealed class UnitConfig : ScriptableObject
    {
        public UnitId UnitId;
        public EntityPoolId EntityPoolId;
        public float Speed;
    }
}