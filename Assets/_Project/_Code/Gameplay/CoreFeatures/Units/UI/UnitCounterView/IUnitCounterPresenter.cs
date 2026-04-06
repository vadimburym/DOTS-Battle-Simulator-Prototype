using System;
using _Project._Code.Gameplay.CoreFeatures.Units.Factory;
using _Project._Code.Locale;
using R3;

namespace _Project._Code.Gameplay.CoreFeatures.Units.UI
{
    public interface IUnitCounterPresenter : IDisposable
    {
        ReadOnlyReactiveProperty<string> CounterText { get; }
    }
}