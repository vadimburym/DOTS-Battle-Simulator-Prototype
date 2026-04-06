using _Project._Code.Locale;

namespace _Project._Code.Gameplay.MetaFeatures.MetaInterface
{
    public interface IMetaInterfacePresenter : IWidgetPresenter
    {
        void OnPlayClicked();
        void OnExitClicked();
    }
}