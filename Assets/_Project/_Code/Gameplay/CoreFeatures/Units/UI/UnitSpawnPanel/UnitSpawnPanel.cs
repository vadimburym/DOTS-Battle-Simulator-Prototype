using System.Linq;
using _Project._Code.Locale;
using UnityEngine;
using UnityEngine.UI;

namespace _Project._Code.Gameplay.CoreFeatures.Units.UI.UnitSpawnPanel
{
    public sealed class UnitSpawnPanel : MonoWidget<IUnitSpawnPanelPresenter>
    {
        [SerializeField] private Button _clearButton;

        [SerializeField] private UnitSpawnButton _itemPrefab;
        [SerializeField] private Transform _container;
        private UIGameObjectMemoryPool<UnitSpawnButton> _pool;

        private void Awake()
        {
            _pool = new(
                _itemPrefab,
                _container,
                _container.gameObject.GetComponentsInChildren<UnitSpawnButton>().ToList());
        }
        
        private IUnitSpawnPanelPresenter _presenter;

        public override void Initialize(IUnitSpawnPanelPresenter presenter)
        {
            _presenter = presenter;
            for (int i = 0; i < _presenter.Presenters.Count; i++)
            {
                var itemPresenter = _presenter.Presenters[i];
                var view = _pool.SpawnItem();
                view.gameObject.SetActive(false);
                view.Initialize(itemPresenter);
                view.gameObject.SetActive(true);
            }
        }

        private void OnEnable()
        {
            if (_presenter == null)
                return;
            _clearButton.onClick.AddListener(_presenter.OnClearSpawnDataClicked);
        }

        private void OnDisable()
        {
            if (_presenter == null)
                return;
            _clearButton.onClick.RemoveListener(_presenter.OnClearSpawnDataClicked);
        }
    }
}