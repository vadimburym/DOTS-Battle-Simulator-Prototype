using UnityEngine;

namespace _Project._Code.Locale
{
    public abstract class MonoWidget<TPresenter> : MonoBehaviour
        where TPresenter : IWidgetPresenter
    {
        public abstract void Initialize(TPresenter presenter);
    }
}