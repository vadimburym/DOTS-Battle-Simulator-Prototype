using _Project._Code.Global.SettingsService.UI.EnumSetting;
using _Project._Code.Global.SettingsService.UI.Settings;
using _Project._Code.Locale;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project._Code.Global.SettingsService.UI
{
    public sealed class SettingsWidget : MonoWidget<ISettingsPresenter>
    {
        [SerializeField] private BoolSettingView _boolSettingViewPrefab;
        [SerializeField] private EnumSettingView _enumSettingViewPrefab;
        [SerializeField] private NumericSettingView _numericSettingViewPrefab;
        [SerializeField] private TMP_Text _titlePrefab;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _quitButton;
        
        [SerializeField] private Transform _container;
        
        private UIGameObjectMemoryPool<BoolSettingView> _boolSettingPool;
        private UIGameObjectMemoryPool<EnumSettingView> _enumSettingPool;
        private UIGameObjectMemoryPool<NumericSettingView> _numericSettingPool;
        private UIGameObjectMemoryPool<TMP_Text> _titlePool;

        private ISettingsPresenter _presenter;
        
        private void Awake()
        {
            _boolSettingPool = new(_boolSettingViewPrefab, _container, null);
            _enumSettingPool = new(_enumSettingViewPrefab, _container, null);
            _numericSettingPool = new(_numericSettingViewPrefab, _container, null);
            _titlePool = new(_titlePrefab, _container, null);
        }
        
        public override void Initialize(ISettingsPresenter presenter)
        {
            _presenter = presenter;
            var groups = presenter.Groups;
            for (int k = 0; k < groups.Count; k++)
            {
                var title = _titlePool.SpawnItem();
                title.text = groups[k].Title;
                var presenters = groups[k].Presenters;
                for (int i = 0; i < presenters.Count; i++)
                {
                    if (presenters[i] is IBoolSettingPresenter boolSettingPresenter)
                    {
                        var boolSetting = _boolSettingPool.SpawnItem();
                        boolSetting.gameObject.SetActive(false);
                        boolSetting.Initialize(boolSettingPresenter);
                        boolSetting.gameObject.SetActive(true);
                    }
                    if (presenters[i] is INumericSettingPresenter numericSettingPresenter)
                    {
                        var numericSetting = _numericSettingPool.SpawnItem();
                        numericSetting.gameObject.SetActive(false);
                        numericSetting.Initialize(numericSettingPresenter);
                        numericSetting.gameObject.SetActive(true);
                    }
                    if (presenters[i] is IEnumSettingPresenter enumSettingPresenter)
                    {
                        var enumSetting = _enumSettingPool.SpawnItem();
                        enumSetting.gameObject.SetActive(false);
                        enumSetting.Initialize(enumSettingPresenter);
                        enumSetting.gameObject.SetActive(true);
                    }
                }
            }
        }

        private void OnEnable()
        {
            if (_presenter == null)
                return;
            _confirmButton.onClick.AddListener(_presenter.OnConfirmButtonClicked);
            _quitButton.onClick.AddListener(_presenter.OnQuitButtonClicked);
        }

        private void OnDisable()
        {
            if (_presenter == null)
                return;
            _confirmButton.onClick.RemoveListener(_presenter.OnConfirmButtonClicked);
            _quitButton.onClick.RemoveListener(_presenter.OnQuitButtonClicked);
        }
    }
}