using System.Collections.Generic;
using _Project._Code.Core.Keys;
using _Project._Code.Global.Settings;
using _Project._Code.Infrastructure;
using _Project._Code.Infrastructure.ApplicationService;

namespace _Project._Code.Global.SettingsService.UI
{
    public sealed class SettingsPresenter : ISettingsPresenter
    {
        public IReadOnlyList<SettingGroup> Groups => _groups;
        
        private readonly List<SettingGroup> _groups;
        private readonly IInputService _inputService;
        private readonly ISettingsService _settingsService;
        private readonly IStateMachine _stateMachine;
        private readonly IApplicationService _applicationService;
        
        public SettingsPresenter(
            List<SettingGroup> groups,
            IInputService inputService,
            ISettingsService settingsService,
            IStateMachine stateMachine,
            IApplicationService applicationService)
        {
            _groups = groups;
            _inputService = inputService;
            _settingsService = settingsService;
            _stateMachine = stateMachine;
            _applicationService = applicationService;
        }

        public void OnQuitButtonClicked()
        {
            if (_stateMachine.CurrentStateId == GameStateId.Meta)
            {
                _applicationService.Quit();
                return;
            }
            _stateMachine.Enter(GameStateId.Meta);
        }
        
        public void OnConfirmButtonClicked()
        {
            _settingsService.Save();
            _inputService.InputEscape();
        }
        
        public void Dispose()
        {
            for (int i = 0; i < _groups.Count; i++)
                _groups[i].Dispose();
        }
    }
}