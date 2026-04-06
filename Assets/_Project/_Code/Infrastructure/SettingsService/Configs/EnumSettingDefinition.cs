using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

namespace _Project._Code.Infrastructure.Settings
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
        public int DefaultValue;

        [ShowInInspector, ReadOnly, TableList]
        [PropertyOrder(10)]
        public List<EnumValueRow> EnumValuesTable => BuildEnumTable();

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
        }

        private Type GetEnumType()
        {
            if (string.IsNullOrWhiteSpace(EnumTypeName))
                return null;

            return Type.GetType(EnumTypeName);
        }

        private List<EnumValueRow> BuildEnumTable()
        {
            var type = GetEnumType();
            var result = new List<EnumValueRow>();

            if (type == null)
                return result;

            foreach (var value in Enum.GetValues(type))
            {
                result.Add(new EnumValueRow
                {
                    Name = Enum.GetName(type, value),
                    Value = Convert.ToInt32(value),
                    IsDefault = Convert.ToInt32(value) == DefaultValue
                });
            }

            return result;
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
    public sealed class EnumValueRow
    {
        [TableColumnWidth(220, Resizable = false)]
        public string Name;
        [TableColumnWidth(80, Resizable = false)]
        public int Value;
        [TableColumnWidth(70, Resizable = false)]
        public bool IsDefault;
    }
}