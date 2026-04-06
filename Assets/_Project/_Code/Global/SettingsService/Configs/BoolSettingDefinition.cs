using System;

namespace _Project._Code.Global.Settings
{
    [Serializable]
    public sealed class BoolSettingDefinition : SettingDefinition
    {
        public bool DefaultValue;
    }
}