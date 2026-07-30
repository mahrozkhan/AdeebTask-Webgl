using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AdeebTask.Models;
using AdeebTask.Core;
using AdeebTask.Services.Persistence;
using AdeebTask.Core.Events;
using AdeebTask.Services.Assets;
using Cysharp.Threading.Tasks;

namespace AdeebTask.UI.Screens
{
    public class EditorScreen : AppScreen
    {
        private IEventBus _eventBus;

        [Header("Toolbox UI")]
        [SerializeField] private GameObject _categoryPanel;
        [SerializeField] private Transform _categoryTabsContainer;
        [SerializeField] private CategoryTabView _categoryTabPrefab;
        [SerializeField] private Button _categoryBackButton;

        [SerializeField] private GameObject _itemsPanel;
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private AssetItemView _itemPrefab;
        [SerializeField] private Button _itemsBackButton;

        [Header("Canvas Settings")]
        [SerializeField] private Image _canvasBackground;

        [Header("Main Actions")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _placeItemButton;

        private List<AssetItemView> _activeItems = new List<AssetItemView>();
        private List<CategoryTabView> _activeTabs = new List<CategoryTabView>();

        private AssetCatalogue _catalogue;
        private AssetCategory _currentViewedCategory;
        private IAssetService _assetService;
        private AssetHandle<Sprite> _currentBgHandle;

        private void Start()
        {
            LoadServices();
            if (_backButton != null) _backButton.onClick.AddListener(() => _eventBus.Publish(new EditorQuitRequestedEvent()));
            if (_saveButton != null) _saveButton.onClick.AddListener(() => _eventBus.Publish(new SaveProjectRequestedEvent(true)));

            if (_placeItemButton != null) _placeItemButton.onClick.AddListener(ShowCategoryPanel);
            if (_categoryBackButton != null) _categoryBackButton.onClick.AddListener(HideAllPanels);
            if (_itemsBackButton != null) _itemsBackButton.onClick.AddListener(ShowCategoryPanel);
        }

        void LoadServices()
        {
            if (_assetService == null)
                _assetService = ServiceLocator.Get<IAssetService>();
            if (_eventBus == null)
            {
                _eventBus = ServiceLocator.Get<IEventBus>();
                _eventBus.Subscribe<EditorModeChangedEvent>(HandleModeChanged);
                _eventBus.Subscribe<BackgroundUpdatedEvent>(HandleBackgroundUpdated);
                _eventBus.Subscribe<PageLoadedEvent>(HandlePageLoaded);
            }
        }
        private void OnDisable()
        {
            if (_activeTabs != null)
            {
                foreach (var tab in _activeTabs)
                {
                    if (tab != null) tab.ClearMemory();
                }
            }
            if (_activeItems != null)
            {
                foreach (var item in _activeItems)
                {
                    if (item != null) item.ClearMemory();
                }
            }

            if (_currentBgHandle != null && _assetService != null)
            {
                _assetService.Release(_currentBgHandle.Key);
                _currentBgHandle = null;
            }
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<EditorModeChangedEvent>(HandleModeChanged);
                _eventBus.Unsubscribe<BackgroundUpdatedEvent>(HandleBackgroundUpdated);
                _eventBus.Unsubscribe<PageLoadedEvent>(HandlePageLoaded);
            }
            OnDisable(); // Failsafe
        }

        private void HandleModeChanged(EditorModeChangedEvent evt)
        {
            if (_placeItemButton != null) _placeItemButton.gameObject.SetActive(!evt.IsViewOnly);
            if (_saveButton != null) _saveButton.gameObject.SetActive(!evt.IsViewOnly);
            if (evt.IsViewOnly) HideAllPanels();
        }

        public void InitializeToolbox()
        {
            LoadServices();
            var cache = ServiceLocator.Get<ILocalCacheService>();
            _catalogue = cache.GetCachedAssetCatalogue();

            if (_catalogue == null || _catalogue.categories == null || _catalogue.categories.Count == 0) return;

            // Object pooling for Tabs
            for (int i = 0; i < Mathf.Max(_catalogue.categories.Count, _activeTabs.Count); i++)
            {
                if (i < _catalogue.categories.Count)
                {
                    CategoryTabView tabView;
                    if (i < _activeTabs.Count)
                    {
                        tabView = _activeTabs[i];
                    }
                    else
                    {
                        if (_categoryTabPrefab == null) break;
                        tabView = Instantiate(_categoryTabPrefab, _categoryTabsContainer);
                        _activeTabs.Add(tabView);
                    }

                    var category = _catalogue.categories[i];
                    tabView.Setup(category.name, category.iconAddressableKey, () => ShowCategory(category), _assetService);
                    tabView.gameObject.SetActive(true);
                }
                else
                {
                    if (_activeTabs[i] != null) _activeTabs[i].gameObject.SetActive(false);
                }
            }

            // Hide panels initially
            HideAllPanels();
        }

        private void ShowCategory(AssetCategory category)
        {
            _currentViewedCategory = category;

            // Object pooling for items
            for (int i = 0; i < Mathf.Max(category.items.Count, _activeItems.Count); i++)
            {
                if (i < category.items.Count)
                {
                    AssetItemView itemView;
                    if (i < _activeItems.Count)
                    {
                        itemView = _activeItems[i];
                    }
                    else
                    {
                        itemView = Instantiate(_itemPrefab, _itemsContainer);
                        itemView.OnItemClicked += HandleItemClicked;
                        _activeItems.Add(itemView);
                    }
                    itemView.gameObject.SetActive(true);
                    itemView.Setup(category.items[i], _assetService);
                }
                else
                {
                    if (_activeItems[i] != null) _activeItems[i].gameObject.SetActive(false);
                }
            }

            // Transition UI
            if (_categoryPanel != null) _categoryPanel.SetActive(false);
            if (_itemsPanel != null) _itemsPanel.SetActive(true);
        }

        private void HandleItemClicked(string addressableKey)
        {
            HideAllPanels();

            if (_currentViewedCategory != null && _currentViewedCategory.name.Equals("Backgrounds", StringComparison.OrdinalIgnoreCase))
            {
                _eventBus.Publish(new Core.Events.SetBackgroundRequestedEvent(addressableKey));
            }
            else
            {
                _eventBus.Publish(new Core.Events.SpawnObjectRequestedEvent(addressableKey));
            }
        }

        private void HandleBackgroundUpdated(BackgroundUpdatedEvent evt)
        {
            LoadBackgroundAsync(evt.AddressableKey).Forget();
        }

        private void HandlePageLoaded(PageLoadedEvent evt)
        {
            bool hasBgImage = !string.IsNullOrEmpty(evt.PageData.backgroundKey);

            if (!hasBgImage)
            {
                // Apply solid color and clear sprite if no image is present
                if (!string.IsNullOrEmpty(evt.PageData.backgroundColorHex))
                {
                    if (Enum.TryParse<CanvasColorType>(evt.PageData.backgroundColorHex, out var parsedType))
                    {
                        if (_canvasBackground != null)
                        {
                            _canvasBackground.color = parsedType.ToColor();
                            _canvasBackground.sprite = null;
                        }
                    }
                    else if (ColorUtility.TryParseHtmlString(evt.PageData.backgroundColorHex, out var parsedColor))
                    {
                        if (_canvasBackground != null)
                        {
                            _canvasBackground.color = parsedColor;
                            _canvasBackground.sprite = null;
                        }
                    }
                }
                
                // Safely release the old background if we are dropping to a solid color
                if (_currentBgHandle != null && _assetService != null)
                {
                    _assetService.Release(_currentBgHandle.Key);
                    _currentBgHandle = null;
                }
            }
            else
            {
                // We have a background image, let LoadBackgroundAsync swap seamlessly
                LoadBackgroundAsync(evt.PageData.backgroundKey).Forget();
            }
        }

        private async UniTaskVoid LoadBackgroundAsync(string key)
        {
            if (string.IsNullOrEmpty(key) || _canvasBackground == null || _assetService == null) return;
            if (_currentBgHandle != null && _currentBgHandle.Key == key) return;

            var newHandle = await _assetService.AcquireAsync<Sprite>(key);
            
            if (newHandle != null && newHandle.Asset != null)
            {
                _canvasBackground.color = Color.white; // Reset tint
                _canvasBackground.sprite = newHandle.Asset;

                // Safely release the old handle AFTER the swap is complete
                if (_currentBgHandle != null)
                {
                    _assetService.Release(_currentBgHandle.Key);
                }
                
                _currentBgHandle = newHandle;
            }
        }

        private void ShowCategoryPanel()
        {
            if (_categoryPanel != null) _categoryPanel.SetActive(true);
            if (_itemsPanel != null) _itemsPanel.SetActive(false);
        }

        private void HideAllPanels()
        {
            if (_categoryPanel != null) _categoryPanel.SetActive(false);
            if (_itemsPanel != null) _itemsPanel.SetActive(false);
        }

        public void SetBackgroundColor(Color color)
        {
            if (_canvasBackground != null)
            {
                _canvasBackground.color = color;
                _canvasBackground.sprite = null; // Clear image if it's solid color
            }
        }
    }
}
