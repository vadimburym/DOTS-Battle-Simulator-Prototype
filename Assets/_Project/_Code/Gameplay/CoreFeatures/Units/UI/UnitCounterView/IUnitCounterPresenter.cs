using _Project._Code.Gameplay.CoreFeatures.Units.Factory;
using _Project._Code.Locale;
using R3;

namespace _Project._Code.Gameplay.CoreFeatures.Units.UI
{
    public interface IUnitCounterPresenter : IWidgetPresenter
    {
        ReadOnlyReactiveProperty<string> CounterText { get; }
    }
}