using System;

namespace _Project._Code.Infrastructure.Settings
{
    [Serializable]
    public sealed class BoolSettingDefinition : SettingDefinition
    {
        public bool DefaultValue;
    }
}