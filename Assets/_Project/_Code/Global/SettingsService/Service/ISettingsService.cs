using System;
using _Project._Code.Core.Keys;
using R3;

namespace _Project._Code.Global.Settings
{
    public interface ISettingsService : IDisposable
    {
        bool Contains(SettingId id);
        Type GetValueType(SettingId id);
        
        void Set<T>(SettingId id, T value);
        bool TrySet<T>(SettingId id, T value);
        T Get<T>(SettingId id);
        bool TryGet<T>(SettingId id, out T value);

        ReadOnlyReactiveProperty<T> Observe<T>(SettingId id);

        void SetEnum<TEnum>(SettingId id, TEnum value) where TEnum : struct, Enum;
        TEnum GetEnum<TEnum>(SettingId id) where TEnum : struct, Enum;
        Observable<TEnum> ObserveEnum<TEnum>(SettingId id) where TEnum : struct, Enum;

        void Save();
        void Load();
        void DeleteSaved();
    }
}