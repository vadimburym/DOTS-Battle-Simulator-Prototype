using System;
using System.Collections.Generic;
using _Project._Code.Core.Keys;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace _Project._Code.Infrastructure.StaticData
{
    [CreateAssetMenu(
        fileName = "WidgetStaticData",
        menuName = "_Project/StaticData/New WidgetStaticData"
    )]
    public sealed class WidgetStaticData : SerializedScriptableObject
    {
        [OdinSerialize] public Dictionary<GameStateId, ScreenId> MainScreens;
        [OdinSerialize] public Dictionary<ScreenId, WidgetId[]> ScreenData;
    }
}