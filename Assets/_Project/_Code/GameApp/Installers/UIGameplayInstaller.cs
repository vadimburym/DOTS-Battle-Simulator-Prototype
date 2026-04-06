using _Project._Code.Core.Abstractions;
using _Project._Code.Core.Contracts;
using _Project._Code.Gameplay.CoreFeatures.Units.UI;
using _Project._Code.Gameplay.CoreFeatures.Units.UI.UnitSpawnPanel;
using _Project._Code.Infrastructure.StaticData;
using _Project._Code.Infrastructure.StaticData.Units;
using _Project._Code.Locale;
using UnityEngine;
using VContainer;

namespace _Project._Code.GameApp.Installers
{
    [CreateAssetMenu(fileName = "UIGameplayInstaller", menuName = "_Project/Installers/new UIGameplayInstaller")]
    public sealed class UIGameplayInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private WidgetConfig _unitCounterConfig;
        [SerializeField] private UnitSpawnPanelConfig _unitSpawnPanelConfig;
        
        public override void Register(IContainerBuilder builder)
        {
            builder.Register<UnitCounterShower>(Lifetime.Singleton)
                .As<IWidgetShower, IInit, IDispose>().WithParameter(_unitCounterConfig);
            builder.Register<UnitSpawnPanelShower>(Lifetime.Singleton)
                .As<IWidgetShower, IInit, IDispose>().WithParameter(_unitSpawnPanelConfig);
        }
    }
}