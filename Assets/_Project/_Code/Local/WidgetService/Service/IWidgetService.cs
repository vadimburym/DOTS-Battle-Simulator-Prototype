using System;
using _Project._Code.Core.Keys;

namespace _Project._Code.Locale
{
    public interface IWidgetService
    {
        event Action<ScreenId> OnScreenChanged;
        bool IsMainScreen { get; }
        void ShowScreen(ScreenId name);
        void HideCurrentScreen();
    }
}