using _Project._Code.Core.Abstractions;
using _Project._Code.Core.Contracts;
using _Project._Code.Gameplay.MetaFeatures.MetaInterface;
using _Project._Code.Global.SettingsService.UI;
using _Project._Code.Infrastructure.StaticData;
using _Project._Code.Locale;
using UnityEngine;
using VContainer;

namespace _Project._Code.GameApp.Installers
{
    [CreateAssetMenu(fileName = "UIMetaInstaller", menuName = "_Project/Installers/new UIMetaInstaller")]
    public sealed class MetaUIInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private WidgetConfig _metaInterfaceConfig;
        [SerializeField] private SettingsWidgetConfig _settingsWidgetConfig;
        
        public override void Register(IContainerBuilder builder)
        {
            builder.Register<MetaInterfaceShower>(Lifetime.Singleton)
                .As<IWidgetShower, IInit, IDispose>().WithParameter(_metaInterfaceConfig);
            builder.Register<SettingsWidgetShower>(Lifetime.Singleton)
                .As<IWidgetShower, IInit, IDispose>().WithParameter(_settingsWidgetConfig);
        }
    }
}