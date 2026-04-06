using System;
using System.Collections.Generic;
using _Project._Code.Core.Keys;
using _Project._Code.Infrastructure;
using _Project._Code.Infrastructure.StaticData;
using _Project._Code.Infrastructure.StaticData._Root;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace _Project._Code.Locale
{
    public sealed class WidgetService : IWidgetService
    {
        public event Action<ScreenId> OnScreenChanged;

        private readonly DepthOfField _depth;
        private readonly Dictionary<WidgetId, IWidgetShower> _widgets = new();
        private readonly Queue<ScreenId> _screenQueue = new();

        public bool IsMainScreen => _currentScreen == _mainScreenId;
        private readonly ScreenId _mainScreenId;
        private ScreenId _currentScreen;

        private readonly WidgetStaticData _staticData;

        public WidgetService(
            IReadOnlyList<IWidgetShower> widgets,
            StaticDataService staticDataService,
            IMainCameraService mainCameraService,
            IStateMachine stateMachine)
        {
            for (int i = 0; i < widgets.Count; i++)
                _widgets.Add(widgets[i].ID, widgets[i]);

            _staticData = staticDataService.WidgetStaticData;
            _mainScreenId = _staticData.MainScreens[stateMachine.CurrentStateId];
            _currentScreen = _mainScreenId;
            var volume = mainCameraService.MainCameraTransform.GetComponent<Volume>();
            if (volume != null && volume.profile.TryGet<DepthOfField>(out var depth))
            {
                _depth = depth;
                _depth.active = false;
            }
        }

        public void ShowScreen(ScreenId name)
        {
            if (name == _mainScreenId)
                return;
            if (name == _currentScreen)
                return;
            if (_currentScreen == _mainScreenId)
            {
                HideScreenInternal(_mainScreenId);
                SetDepthOfField();
            }
            else
            {
                _screenQueue.Enqueue(_currentScreen);
                HideScreenInternal(_currentScreen);
            }

            _currentScreen = name;
            ShowScreenInternal(name);
        }

        public void HideCurrentScreen()
        {
            if (_currentScreen == _mainScreenId)
                return;

            HideScreenInternal(_currentScreen);
            if (_screenQueue.TryDequeue(out var screen))
            {
                _currentScreen = screen;
                ShowScreenInternal(screen);
            }
            else
            {
                _currentScreen = _mainScreenId;
                ShowScreenInternal(_mainScreenId);
                UnsetDepthOfField();
            }
        }

        private void ShowScreenInternal(ScreenId screenId)
        {
            var widgets = _staticData.ScreenData[screenId];
            for (int i = 0; i < widgets.Length; i++)
            {
                _widgets[widgets[i]].Show();
            }
            OnScreenChanged?.Invoke(screenId);
        }

        private void HideScreenInternal(ScreenId name)
        {
            var widgets = _staticData.ScreenData[name];
            for (int i = 0; i < widgets.Length; i++)
            {
                _widgets[widgets[i]].Hide();
            }
        }

        private void SetDepthOfField()
        {
            if (_depth != null)
                _depth.active = true;
        }

        private void UnsetDepthOfField()
        {
            if (_depth != null)
                _depth.active = false;
        }
    }
}
