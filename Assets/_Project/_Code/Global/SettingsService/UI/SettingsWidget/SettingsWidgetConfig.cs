using System;
using _Project._Code.Core.Keys;
using _Project._Code.Infrastructure.StaticData;
using UnityEngine;

namespace _Project._Code.Global.SettingsService.UI
{
    [CreateAssetMenu(fileName = "SettingsWidgetConfig", menuName = "_Project/new SettingsWidgetConfig")]
    public sealed class SettingsWidgetConfig : WidgetConfig
    {
        public SettingGroupConfig[] Groups;      
    }
    
    [Serializable]
    public struct SettingGroupConfig
    {
        public string GroupTitleKey;
        public SettingId[] Settings;
    }
}