using _Project._Code.Infrastructure.Settings;
using Unity.Entities;
using VContainer;

namespace _Project._Code.Infrastructure.ApplicationService
{
    [DisableAutoCreation]
    public partial class ApplicationSettingsSyncSystem : SystemBase
    {
        private IApplicationService _applicationService;
        private ISettingsService _settingsService;
        
        [Inject]
        private void Construct(
            IApplicationService applicationService,
            ISettingsService settingsService)
        {
            _applicationService = applicationService;
            _settingsService = settingsService;
        }

        protected override void OnUpdate() { }
    }
}