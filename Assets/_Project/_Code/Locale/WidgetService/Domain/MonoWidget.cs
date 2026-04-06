using System;
using UnityEngine;

namespace _Project._Code.Locale
{
    public abstract class MonoWidget<TPresenter> : MonoBehaviour
        where TPresenter : IDisposable
    {
        public abstract void Initialize(TPresenter presenter);
    }
}