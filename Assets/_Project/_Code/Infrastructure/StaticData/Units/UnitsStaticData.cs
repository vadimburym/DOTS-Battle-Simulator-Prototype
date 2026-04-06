using System.Collections.Generic;
using _Project._Code.Core.Keys;
using _Project.Code.EditorTools;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _Project._Code.Infrastructure.StaticData.Units
{
    [CreateAssetMenu(fileName = nameof(UnitsStaticData), menuName = "_Project/Core/New UnitsStaticData")]
    public sealed class UnitsStaticData : SerializedScriptableObject
    {
        [OdinSerialize] private Dictionary<UnitId, AssetReferenceT<UnitConfig>> _units;
        
        public AssetReferenceT<UnitConfig> GetUnitData(UnitId unitId) 
            => _units[unitId];
    }
}