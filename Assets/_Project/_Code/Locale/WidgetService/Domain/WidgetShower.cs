using System;
using _Project._Code.Core.Contracts;
using _Project._Code.Core.Keys;
using _Project._Code.Infrastructure;
using _Project._Code.Infrastructure.StaticData;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using Object = UnityEngine.Object;

namespace _Project._Code.Locale
{
    public abstract class WidgetShower<TPresenter, TWidget> : IWidgetShower, IInit, IDispose
        where TWidget : MonoWidget<TPresenter>
        where TPresenter : IDisposable
    {
        protected TWidget _view;
        protected TPresenter _presenter;
        public WidgetId ID => _id;
        private readonly WidgetId _id;
        private readonly AssetReference _widgetReference;
        private readonly bool _showOnStart;
        
        [Inject] private readonly ITransformProvider _transformProvider;
        [Inject] private readonly IAddressableService _addressableService;

        protected WidgetShower(WidgetConfig config)
        {
            _widgetReference = config.WidgetReference;
            _id = config.WidgetId;
            _showOnStart = config.ShowOnStart;
        }

        void IInit.Init()
        {
            var prefab = _addressableService.GetLoadedObject<GameObject>(_widgetReference);
            var transform = _transformProvider.GetTransform(TransformId.OverlayCanvas);
            _view = Object.Instantiate(prefab, transform).GetComponent<TWidget>();
            _view.gameObject.SetActive(false);
            _presenter = CreatePresenter();
            _view.Initialize(_presenter);
            if (_showOnStart) Show();
        }

        protected abstract TPresenter CreatePresenter();
        public void Show() => _view.gameObject.SetActive(true);
        public void Hide() => _view.gameObject.SetActive(false);
        public void Dispose() => _presenter.Dispose();
    }
}