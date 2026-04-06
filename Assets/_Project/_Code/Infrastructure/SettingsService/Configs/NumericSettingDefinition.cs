using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project._Code.Infrastructure.Settings
{
    [Serializable]
    public sealed class NumericSettingDefinition : SettingDefinition
    {
        public float DefaultValue = 0f;
        public float Min = 0f;
        public float Max = 1f;
        [OnValueChanged(nameof(OnWholeNumbersChanged))]
        public bool WholeNumbers = false;

        public void OnWholeNumbersChanged()
        {
            if (WholeNumbers)
            {
                DefaultValue = Mathf.Round(DefaultValue);
                Min = Mathf.Round(Min);
                Max = Mathf.Round(Max);
            }
        }
    }
}