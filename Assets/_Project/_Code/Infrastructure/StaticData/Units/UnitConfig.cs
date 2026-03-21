using _Project._Code.Core.Keys;
using UnityEngine;

namespace _Project._Code.Infrastructure.StaticData.Units
{
    [CreateAssetMenu(fileName = nameof(UnitConfig), menuName = "_Project/Core/New UnitConfig")]
    public sealed class UnitConfig : ScriptableObject
    {
        [Header("Entity")]
        public UnitId UnitId;
        public EntityPoolId EntityPoolId;
        [Header("AI")]
        public BehaviourTreeId BehaviourTreeId;
        [Header("MovementStats")]
        public float Speed = 0f;
        public float RotationSpeed = 0f;
        [Header("Footprint")]
        public byte FootprintX = 1;
        public byte FootprintY = 1;
        [Header("AttackStats")]
        public float AttackInterval = 3f;
        public byte AttackRangeCells = 2;
        public ushort Damage = 5;
        [Header("EyeSensorStats")]
        public float DetectRadius = 10f;
        public float ChaseRadius = 15f;
        public float UpdateNearestInterval = 3f;
        public float ScanInterval = 0.3f;
    }
}