using System;
using System.Collections.Generic;
using _Project._Code.Core.Keys;
using R3;
using UnityEngine;

namespace _Project._Code.Global.Settings
{
    public sealed class SettingsService : ISettingsService
    {
        private const string KeyPrefix = "app.settings.";
        private readonly Dictionary<SettingId, ISettingEntry> _settings = new();

        public bool Contains(SettingId id)
            => _settings.ContainsKey(id);

        public Type GetValueType(SettingId id)
        {
            if (!_settings.TryGetValue(id, out var entry))
                throw new KeyNotFoundException($"Setting '{id}' is not registered.");
            return entry.ValueType;
        }

        public void WarmUp(SettingsPipeline pipeline)
        {
            foreach (var setting in pipeline.Settings)
            {
                switch (setting)
                {
                    case BoolSettingDefinition boolDef:
                        RegisterOrSet(boolDef.Id, boolDef.DefaultValue);
                        break;
                    case NumericSettingDefinition numDef:
                        RegisterOrSet(numDef.Id, numDef.DefaultValue);
                        break;
                    case EnumSettingDefinition enumDef:
                        RegisterOrSet(enumDef.Id, enumDef.DefaultValue);
                        break;
                }
            }
        }

        public void RegisterOrSet<T>(SettingId id, T value)
        {
            if (_settings.TryGetValue(id, out var existing))
            {
                if (existing is not SettingEntry<T> typedEntry)
                    throw BuildTypeMismatchException(id, typeof(T), existing.ValueType);

                typedEntry.Set(value);
                return;
            }

            _settings.Add(id, new SettingEntry<T>(id, value));
        }

        public void Set<T>(SettingId id, T value)
        {
            var entry = GetEntry<T>(id);
            entry.Set(value);
        }

        public bool TrySet<T>(SettingId id, T value)
        {
            if (!_settings.TryGetValue(id, out var entry))
                return false;

            if (entry is not SettingEntry<T> typedEntry)
                return false;

            typedEntry.Set(value);
            return true;
        }

        public T Get<T>(SettingId id)
        {
            return GetEntry<T>(id).Get();
        }

        public bool TryGet<T>(SettingId id, out T value)
        {
            if (_settings.TryGetValue(id, out var entry) &&
                entry is SettingEntry<T> typedEntry)
            {
                value = typedEntry.Get();
                return true;
            }

            value = default;
            return false;
        }

        public ReadOnlyReactiveProperty<T> Observe<T>(SettingId id)
        {
            return GetEntry<T>(id).Observable;
        }

        public void SetEnum<TEnum>(SettingId id, TEnum value)
            where TEnum : struct, Enum
        {
            Set(id, Convert.ToInt32(value));
        }

        public TEnum GetEnum<TEnum>(SettingId id)
            where TEnum : struct, Enum
        {
            int value = Get<int>(id);
            return (TEnum)Enum.ToObject(typeof(TEnum), value);
        }

        public Observable<TEnum> ObserveEnum<TEnum>(SettingId id)
            where TEnum : struct, Enum
        {
            return Observe<int>(id)
                .Select(value => (TEnum)Enum.ToObject(typeof(TEnum), value));
        }

        public void Save()
        {
            foreach (var pair in _settings)
                pair.Value.Save(GetKey(pair.Key));
            PlayerPrefs.Save();
        }

        public void Load()
        {
            foreach (var pair in _settings)
                pair.Value.Load(GetKey(pair.Key));
        }

        public void DeleteSaved()
        {
            foreach (var pair in _settings)
                PlayerPrefs.DeleteKey(GetKey(pair.Key));
        }

        public void Dispose()
        {
            foreach (var pair in _settings)
                pair.Value.Dispose();

            _settings.Clear();
        }

        private static string GetKey(SettingId id)
            => $"{KeyPrefix}{id}";

        private SettingEntry<T> GetEntry<T>(SettingId id)
        {
            if (!_settings.TryGetValue(id, out var entry))
                throw new KeyNotFoundException($"Setting '{id}' is not registered.");
            if (entry is not SettingEntry<T> typedEntry)
                throw BuildTypeMismatchException(id, typeof(T), entry.ValueType);
            return typedEntry;
        }

        private static InvalidOperationException BuildTypeMismatchException(
            SettingId id,
            Type requestedType,
            Type actualType)
        {
            return new InvalidOperationException(
                $"Setting '{id}' has type '{actualType.Name}', requested '{requestedType.Name}'.");
        }

        private interface ISettingEntry : IDisposable
        {
            SettingId Id { get; }
            Type ValueType { get; }
            void Save(string key);
            void Load(string key);
        }

        private sealed class SettingEntry<T> : ISettingEntry
        {
            public SettingId Id { get; }
            public Type ValueType => typeof(T);
            public ReadOnlyReactiveProperty<T> Observable => _value;

            private readonly ReactiveProperty<T> _value;
            private readonly T _defaultValue;

            public SettingEntry(SettingId id, T initialValue)
            {
                Id = id;
                _value = new ReactiveProperty<T>(initialValue);
                _defaultValue = initialValue;
            }

            public T Get() => _value.Value;
            public void Set(T value) => _value.Value = value;
            public void Dispose() => _value.Dispose();
            public void Save(string key)
            {
                if (typeof(T) == typeof(bool))
                {
                    bool value = (bool)(object)_value.Value;
                    PlayerPrefs.SetInt(key, value ? 1 : 0);
                    return;
                }
                if (typeof(T) == typeof(int))
                {
                    int value = (int)(object)_value.Value;
                    PlayerPrefs.SetInt(key, value);
                    return;
                }
                if (typeof(T) == typeof(float))
                {
                    float value = (float)(object)_value.Value;
                    PlayerPrefs.SetFloat(key, value);
                    return;
                }
                throw new NotSupportedException(
                    $"Saving setting type '{typeof(T).Name}' is not supported.");
            }

            public void Load(string key)
            {
                if (!PlayerPrefs.HasKey(key))
                {
                    _value.Value = _defaultValue;
                    return;
                }
                if (typeof(T) == typeof(bool))
                {
                    object value = PlayerPrefs.GetInt(key) != 0;
                    _value.Value = (T)value;
                    return;
                }
                if (typeof(T) == typeof(int))
                {
                    object value = PlayerPrefs.GetInt(key);
                    _value.Value = (T)value;
                    return;
                }
                if (typeof(T) == typeof(float))
                {
                    object value = PlayerPrefs.GetFloat(key);
                    _value.Value = (T)value;
                    return;
                }
                throw new NotSupportedException(
                    $"Loading setting type '{typeof(T).Name}' is not supported.");
            }
        }
    }
}
