using _Project._Code.Core.Keys;
using UnityEngine;

namespace _Project._Code.Infrastructure.StaticData.Units
{
    [CreateAssetMenu(fileName = nameof(UnitSpawnPanelConfig), menuName = "_Project/StaticData/New UnitSpawnPanelConfig")]
    public class UnitSpawnPanelConfig : WidgetConfig
    {
        public UnitId[] UnitsToShow;
    }
}