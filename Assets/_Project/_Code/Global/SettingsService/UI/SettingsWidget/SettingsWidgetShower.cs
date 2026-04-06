using System;
using System.Collections.Generic;
using _Project._Code.Core.Keys;
using _Project._Code.GameApp;
using _Project._Code.Global.Settings;
using _Project._Code.Global.SettingsService.UI.EnumSetting;
using _Project._Code.Global.SettingsService.UI.Settings;
using _Project._Code.Infrastructure;
using _Project._Code.Infrastructure.ApplicationService;
using _Project._Code.Locale;

namespace _Project._Code.Global.SettingsService.UI
{
    public sealed class SettingsWidgetShower : WidgetShower<ISettingsPresenter, SettingsWidget>
    {
        private readonly ISettingsService _settingsService;
        private readonly IApplicationService _applicationService;
        private readonly SettingsWidgetConfig _config;
        private readonly IInputService _inputService;
        private readonly IStateMachine _stateMachine;
        
        public SettingsWidgetShower(
            ISettingsService settingsService,
            IApplicationService applicationService,
            IInputService inputService,
            IStateMachine stateMachine,
            SettingsWidgetConfig config) : base(config)
        {
            _config = config;
            _stateMachine = stateMachine;
            _settingsService = settingsService;
            _inputService = inputService;
            _applicationService = applicationService;
        }

        protected override ISettingsPresenter CreatePresenter()
        {
            var staticData = BootstrapContext.Instance.SettingsPipeline;
            var groupsData = _config.Groups;
            var groups = new List<SettingGroup>();
            for (int i = 0; i < groupsData.Length; i++)
            {
                var groupData = groupsData[i];
                var group = new SettingGroup(groupData.GroupTitleKey);
                var groupSettings = groupData.Settings;
                for (int k = 0; k < groupSettings.Length; k++)
                {
                    var setting = groupSettings[k];
                    var valueType = _settingsService.GetValueType(setting);
                    IDisposable presenter = valueType switch
                    {
                        var t when t == typeof(bool)
                            => new BoolSettingPresenter(
                                setting,
                                _settingsService),
                        var t when setting == SettingId.Resolution
                            => new ResolutionSettingPresenter(
                                setting,
                                _applicationService,
                                _settingsService),
                        var t when t == typeof(int)
                            => new EnumSettingPresenter(
                                setting,
                                (EnumSettingDefinition)staticData.GetSettingDefinition(setting),
                                _settingsService),
                        var t when t == typeof(float)
                            => new NumericSettingPresenter(
                                setting,
                                (NumericSettingDefinition)staticData.GetSettingDefinition(setting),
                                _settingsService),
                        _ => throw new NotSupportedException($"Unsupported setting type: {valueType}")
                    };
                    group.Add(presenter);
                }
                groups.Add(group);
            }

            return new SettingsPresenter(groups, _inputService, _settingsService, _stateMachine, _applicationService);
        }
    }
}