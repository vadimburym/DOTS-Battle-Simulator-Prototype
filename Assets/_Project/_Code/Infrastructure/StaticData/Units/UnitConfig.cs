using _Project._Code.Core.Keys;
using _Project.Code.EditorTools;
using UnityEngine;

namespace _Project._Code.Infrastructure.StaticData.Units
{
    [CreateAssetMenu(fileName = nameof(UnitConfig), menuName = "_Project/Core/New UnitConfig")]
    public sealed class UnitConfig : ScriptableObject
#if UNITY_EDITOR
    ,IEditorDataPreview
#endif
    {
#if UNITY_EDITOR
        public Sprite EditorPreview => Icon;        
#endif
        public Sprite Icon;
        
        public UnitId UnitId;
        public EntityPoolId EntityPoolId;
        
        public BehaviourTreeId BehaviourTreeId;
        
        public float Speed = 0f;
        public float RotationSpeed = 0f;
        
        public byte FootprintX = 1;
        public byte FootprintY = 1;
        
        public ushort MaxHealth = 100;
        
        public float AttackInterval = 3f;
        public byte AttackRangeCells = 2;
        public ushort Damage = 5;
        
        public float DetectRadius = 10f;
        public float ChaseRadius = 15f;
        public float UpdateNearestInterval = 3f;
        public float ScanInterval = 0.3f;
    }
}