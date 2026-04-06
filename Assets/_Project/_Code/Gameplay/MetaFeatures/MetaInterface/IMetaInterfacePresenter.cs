using System;
using _Project._Code.Locale;

namespace _Project._Code.Gameplay.MetaFeatures.MetaInterface
{
    public interface IMetaInterfacePresenter : IDisposable
    {
        void OnPlayClicked();
        void OnExitClicked();
    }
}