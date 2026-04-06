using System;
using _Project._Code.Core.Keys;

namespace _Project._Code.Infrastructure.Settings
{
    [Serializable]
    public abstract class SettingDefinition
    {
        public SettingId Id;
    }
}