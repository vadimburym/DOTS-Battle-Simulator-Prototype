using System;
using _Project._Code.Locale;
using UnityEngine;
using UnityEngine.UI;

namespace _Project._Code.Gameplay.MetaFeatures.MetaInterface
{
    public sealed class MetaInterfaceWidget : MonoWidget<IMetaInterfacePresenter>
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _exitButton;
        
        private IMetaInterfacePresenter _presenter;

        public override void Initialize(IMetaInterfacePresenter presenter)
        {
            _presenter = presenter;
        }

        public void OnEnable()
        {
            if (_presenter == null)
                return;
            _playButton.onClick.AddListener(_presenter.OnPlayClicked);
            _exitButton.onClick.AddListener(_presenter.OnExitClicked);
        }

        public void OnDisable()
        {
            if (_presenter == null)
                return;
            _playButton.onClick.RemoveListener(_presenter.OnPlayClicked);
            _exitButton.onClick.RemoveListener(_presenter.OnExitClicked);
        }
    }
}