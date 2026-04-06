using _Project._Code.Core.Keys;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _Project._Code.Infrastructure.StaticData
{
    [CreateAssetMenu(fileName = nameof(WidgetConfig), menuName = "_Project/StaticData/New WidgetConfig")]
    public class WidgetConfig : ScriptableObject
    {
        public WidgetId WidgetId;
        public AssetReferenceGameObject WidgetReference;
        public bool ShowOnStart;
    }
}