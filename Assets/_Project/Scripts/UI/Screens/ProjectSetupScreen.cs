using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AdeebTask.UI;
using AdeebTask.Models;
using Cysharp.Threading.Tasks;
using AdeebTask.Core;
using AdeebTask.Core.Events;
using AdeebTask.Services.Assets;
using AdeebTask.Services.Persistence;

namespace AdeebTask.UI.Screens
{
    public class ProjectSetupScreen : AppScreen
    {
        private IEventBus _eventBus;
        private IAssetService _assetService;
        private AssetHandle<Sprite> _currentBgHandle;

        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Image _previewBackground;

        [Header("Background Selection")]
        [SerializeField] private Transform _backgroundsContainer;
        [SerializeField] private AssetItemView _backgroundItemPrefab;

        private string _selectedBackgroundKey = "";
        private List<AssetItemView> _activeBackgroundItems = new List<AssetItemView>();

        private void Awake()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _assetService = ServiceLocator.Get<IAssetService>();
            
            if (_confirmButton != null) _confirmButton.onClick.AddListener(() => _eventBus.Publish(new ProjectSetupConfirmedEvent(_selectedBackgroundKey)));
            if (_cancelButton != null) _cancelButton.onClick.AddListener(() => _eventBus.Publish(new ProjectSetupCancelledEvent()));
        }

        private void OnEnable()
        {
            _selectedBackgroundKey = ""; // Reset on show
            if (_previewBackground != null)
            {
                _previewBackground.sprite = null;
                _previewBackground.color = Color.white;
            }

            RefreshBackgrounds();
        }

        private void OnDisable()
        {
            if (_activeBackgroundItems != null)
            {
                foreach (var item in _activeBackgroundItems)
                {
                    if (item != null) item.ClearMemory();
                }
            }
            ReleaseBackgroundHandle();
        }

        private void RefreshBackgrounds()
        {
            var localCache = ServiceLocator.Get<ILocalCacheService>();
            if (_backgroundsContainer != null && _backgroundItemPrefab != null && localCache != null)
            {
                var catalogue = localCache.GetCachedAssetCatalogue();
                if (catalogue != null && catalogue.categories != null)
                {
                    var bgCategory = catalogue.categories.Find(c => c.name.Equals("Backgrounds", StringComparison.OrdinalIgnoreCase));
                    if (bgCategory != null && bgCategory.items != null)
                    {
                        for (int i = 0; i < Mathf.Max(bgCategory.items.Count, _activeBackgroundItems.Count); i++)
                        {
                            if (i < bgCategory.items.Count)
                            {
                                AssetItemView itemView;
                                if (i < _activeBackgroundItems.Count)
                                {
                                    itemView = _activeBackgroundItems[i];
                                }
                                else
                                {
                                    itemView = Instantiate(_backgroundItemPrefab, _backgroundsContainer);
                                    itemView.OnItemClicked += HandleBackgroundItemClicked;
                                    _activeBackgroundItems.Add(itemView);
                                }
                                itemView.gameObject.SetActive(true);
                                itemView.Setup(bgCategory.items[i], _assetService);
                            }
                            else
                            {
                                if (_activeBackgroundItems[i] != null) _activeBackgroundItems[i].gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
        }

        private void HandleBackgroundItemClicked(string key)
        {
            _selectedBackgroundKey = key;
            LoadBackgroundPreviewAsync(key).Forget();
        }

        private async UniTaskVoid LoadBackgroundPreviewAsync(string key)
        {
            if (_assetService == null || _previewBackground == null) return;
            
            ReleaseBackgroundHandle();

            _currentBgHandle = await _assetService.AcquireAsync<Sprite>(key);
            if (_currentBgHandle != null && _currentBgHandle.Asset != null)
            {
                _previewBackground.color = Color.white;
                _previewBackground.sprite = _currentBgHandle.Asset;
            }
        }

        private void ReleaseBackgroundHandle()
        {
            if (_currentBgHandle != null && _assetService != null)
            {
                _assetService.Release(_currentBgHandle.Key);
                _currentBgHandle = null;
            }
        }

        private void OnDestroy()
        {
            OnDisable(); // Failsafe
        }
    }
}
