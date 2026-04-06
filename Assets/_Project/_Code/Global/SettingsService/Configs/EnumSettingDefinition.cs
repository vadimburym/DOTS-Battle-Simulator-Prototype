using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project._Code.Global.Settings
{
    [Serializable]
    public sealed class EnumSettingDefinition : SettingDefinition
    {
        [LabelText("Enum Type")]
        [ValueDropdown(nameof(GetEnumTypeOptions))]
        [OnValueChanged(nameof(OnEnumTypeChanged))]
        public string EnumTypeName;

        [LabelText("Default Value")]
        [ValueDropdown(nameof(GetEnumValueOptions))]
        [OnValueChanged(nameof(RebuildEnumTable))]
        public int DefaultValue;

        [ShowInInspector, ReadOnly, TableList]
        [PropertyOrder(10)]
        public List<EnumValueRow> EnumValuesTable => _enumValuesTable;
        
        [SerializeField, HideInInspector]
        private List<EnumValueRow> _enumValuesTable = new();

        public string GetEnumName(int value)
        {
            for (int i = 0; i < _enumValuesTable.Count; i++)
            {
                var row = _enumValuesTable[i];
                if (row.Value == value) return row.Name;
            }
            return string.Empty;
        }

        public int GetUpperValue(int value)
        {
            for (int i = 0; i < _enumValuesTable.Count; i++)
            {
                var row = _enumValuesTable[i];
                if (row.Value == value)
                    return i == 0 ? _enumValuesTable[^1].Value : _enumValuesTable[i - 1].Value;
            }
            return value;
        }

        public int GetDownValue(int value)
        {
            for (int i = 0; i < _enumValuesTable.Count; i++)
            {
                var row = _enumValuesTable[i];
                if (row.Value == value)
                    return i == _enumValuesTable.Count - 1 ? _enumValuesTable[0].Value : _enumValuesTable[i + 1].Value;
            }
            return value;
        }
        
        private IEnumerable<ValueDropdownItem<string>> GetEnumTypeOptions()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(SafeGetTypes)
                .Where(t => t.IsEnum)
                .OrderBy(t => t.FullName)
                .Select(t => new ValueDropdownItem<string>(t.FullName, t.AssemblyQualifiedName));
        }

        private IEnumerable<ValueDropdownItem<int>> GetEnumValueOptions()
        {
            var type = GetEnumType();
            if (type == null)
                yield break;

            foreach (var value in Enum.GetValues(type))
            {
                string name = Enum.GetName(type, value);
                int intValue = Convert.ToInt32(value);
                yield return new ValueDropdownItem<int>($"{name} ({intValue})", intValue);
            }
        }

        private void OnEnumTypeChanged()
        {
            var type = GetEnumType();
            if (type == null)
            {
                DefaultValue = 0;
                return;
            }

            var values = Enum.GetValues(type);
            if (values.Length == 0)
            {
                DefaultValue = 0;
                return;
            }

            int current = DefaultValue;
            bool exists = false;

            foreach (var value in values)
            {
                if (Convert.ToInt32(value) == current)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
                DefaultValue = Convert.ToInt32(values.GetValue(0));
            RebuildEnumTable();
        }

        private Type GetEnumType()
        {
            if (string.IsNullOrWhiteSpace(EnumTypeName))
                return null;

            return Type.GetType(EnumTypeName);
        }
        
        private void RebuildEnumTable()
        {
            var type = GetEnumType();
            if (type == null)
            {
                _enumValuesTable.Clear();
                return;
            }
            Array values = Enum.GetValues(type);
            int count = values.Length;
            while (_enumValuesTable.Count < count)
                _enumValuesTable.Add(default);
            while (_enumValuesTable.Count > count)
                _enumValuesTable.RemoveAt(_enumValuesTable.Count - 1);
            for (int i = 0; i < count; i++)
            {
                object value = values.GetValue(i);
                int intValue = Convert.ToInt32(value);

                _enumValuesTable[i] = new EnumValueRow
                {
                    Name = Enum.GetName(type, value),
                    Value = intValue,
                    IsDefault = intValue == DefaultValue
                };
            }
        }

        private static IEnumerable<Type> SafeGetTypes(System.Reflection.Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch
            {
                return Array.Empty<Type>();
            }
        }
    }

    [Serializable]
    public struct EnumValueRow
    {
        [TableColumnWidth(220, Resizable = false)]
        public string Name;
        [TableColumnWidth(80, Resizable = false)]
        public int Value;
        [TableColumnWidth(70, Resizable = false)]
        public bool IsDefault;
    }
}