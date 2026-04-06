using _Project._Code.Core.Contracts;
using _Project._Code.Core.Keys;
using _Project._Code.Infrastructure;

namespace _Project._Code.Locale
{
    public sealed class EscapeWidgetSystem : ITick, IInit, IDispose
    {
        private readonly IInputService _inputService;
        private readonly IWidgetService _widgetService;
        
        public EscapeWidgetSystem(
            IInputService inputService,
            IWidgetService widgetService)
        {
            _inputService = inputService;
            _widgetService = widgetService;
        }
        
        public void Tick()
        {
            if (_inputService.IsEscape)
            {
                CloseCurrentOrOpenSettings();
            }
        }

        public void Init()
        {
            _inputService.OnEscape += CloseCurrentOrOpenSettings;
        }

        private void CloseCurrentOrOpenSettings()
        {
            if (_widgetService.IsMainScreen)
                _widgetService.ShowScreen(ScreenId.GlobalSettings);
            else
                _widgetService.HideCurrentScreen();
        }

        public void Dispose()
        {
            _inputService.OnEscape -= CloseCurrentOrOpenSettings;
        }
    }
}