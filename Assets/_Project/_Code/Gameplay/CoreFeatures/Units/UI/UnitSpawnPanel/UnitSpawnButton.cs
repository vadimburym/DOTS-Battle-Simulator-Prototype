using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Project._Code.Gameplay.CoreFeatures.Units.UI
{
    public sealed class UnitSpawnButton : MonoBehaviour
    {
        [Serializable]
        public sealed class IntButton
        {
            public Button Button;
            public int Value;
        }
        
        [SerializeField] private Image _icon;
        [SerializeField] private IntButton[] _buttons;
        
        private IUnitSpawnButtonPresenter _presenter;

        public void Initialize(IUnitSpawnButtonPresenter presenter)
        {
            _presenter = presenter;
        }

        private void OnEnable()
        {
            if (_presenter == null)
                return;
            _icon.sprite = _presenter.Icon;
            for (int i = 0; i < _buttons.Length; i++)
            {
                var button = _buttons[i];
                button.Button.onClick.AddListener(() => { _presenter.OnSpawnDataClicked(button.Value); });
            }
        }

        private void OnDisable()
        {
            if (_presenter == null)
                return;
            for (int i = 0; i < _buttons.Length; i++)
            {
                var button = _buttons[i];
                button.Button.onClick.RemoveAllListeners();
            }
        }
    }
}