using System.Collections.Generic;
using _Project._Code.Core.Keys;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VadimBurym.DodBehaviourTree;

namespace _Project._Code.Infrastructure.StaticData.AI
{
    [CreateAssetMenu(fileName = nameof(BehaviourTreeStaticData), menuName = "_Project/Core/New BehaviourTreeStaticData")]
    public sealed class BehaviourTreeStaticData : SerializedScriptableObject
    {
        [OdinSerialize] private Dictionary<BehaviourTreeId, AssetReferenceT<BehaviourTreeAsset>> _assets;
        
        public IReadOnlyDictionary<BehaviourTreeId, AssetReferenceT<BehaviourTreeAsset>> Assets => _assets;
        public AssetReferenceT<BehaviourTreeAsset> GetAsset(BehaviourTreeId assetId) 
            => _assets[assetId];
    }
}