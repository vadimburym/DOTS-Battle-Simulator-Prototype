using System.Collections.Generic;
using _Project._Code.Core.Keys;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project._Code.Global.Settings
{
    [CreateAssetMenu(fileName = "SettingsStaticData",menuName = "_Project/new SettingsStaticData")]
    public sealed class SettingsPipeline : ScriptableObject
    {
        [SerializeReference]
        [ListDrawerSettings(ShowFoldout = true)]
        public List<SettingDefinition> Settings = new();

        public SettingDefinition GetSettingDefinition(SettingId settingId)
        {
            for (int i = 0; i < Settings.Count; i++)
                if (Settings[i].Id == settingId) return Settings[i];
            return null;
        }
    }
}