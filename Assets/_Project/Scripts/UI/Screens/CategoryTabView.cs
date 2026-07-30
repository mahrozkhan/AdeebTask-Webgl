using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using AdeebTask.Services.Assets;
using AdeebTask.Core;

namespace AdeebTask.UI.Screens
{
    public class CategoryTabView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private Button _tabButton;
        [SerializeField] private Image _iconImage;

        private IAssetService _assetService;
        private AssetHandle<Sprite> _currentIconHandle;
        private bool _isLoading = false;
        private int _operationId = 0;
        private Action OnTabClicked;

        private void Awake()
        {
            if (_tabButton != null)
                _tabButton.onClick.AddListener(() => OnTabClicked?.Invoke());
        }

        private void OnDestroy()
        {
            ClearMemory();
        }

        public void ClearMemory()
        {
            if (_currentIconHandle != null && _assetService != null)
            {
                _assetService.Release(_currentIconHandle.Key);
                _currentIconHandle = null;
            }
            if (_iconImage != null)
            {
                _iconImage.sprite = null;
            }
        }

        public void Setup(string name, string iconKey, Action onClick, IAssetService assetService)
        {
            _assetService = assetService;
            if (_nameText != null) _nameText.text = name;
            
            OnTabClicked = onClick;
            
            if (!string.IsNullOrEmpty(iconKey))
            {
                LoadIconAsync(iconKey).Forget();
            }
        }

        private async UniTaskVoid LoadIconAsync(string key)
        {
            if (string.IsNullOrEmpty(key) || _iconImage == null || _assetService == null) 
            {
                Debug.LogWarning($"[CategoryTabView] Aborting load for {key}. Key null? {string.IsNullOrEmpty(key)} Image null? {_iconImage == null} AssetService null? {_assetService == null}");
                return;
            }
            
            if (_iconImage != null)
            {
                _iconImage.sprite = null;
                _iconImage.enabled = true;
                _isLoading = true;
                _operationId++;
                PulseLoader(_operationId).Forget();
            }

            Debug.Log($"[CategoryTabView] Attempting to load Sprite with key: {key}");
            
            _currentIconHandle = await _assetService.AcquireAsync<Sprite>(key);
            
                
            if (_currentIconHandle != null && _currentIconHandle.Asset != null)
            {
                _iconImage.sprite = _currentIconHandle.Asset;
                _iconImage.color = new Color(1, 1, 1, 0); // Start transparent
                _iconImage.enabled = true; // Ensure the image component is enabled
                _isLoading = false;

                if (_nameText != null) _nameText.gameObject.SetActive(false); // Hide the text
                Debug.Log($"[CategoryTabView] Successfully loaded and applied Sprite for key: {key}");
                
                FadeInIcon().Forget();
            }
            else
            {
                Debug.LogError($"[CategoryTabView] Failed to load Sprite for key: {key}. Handle or Asset was null.");
            }
        }

        private async UniTaskVoid PulseLoader(int opId)
        {
            float time = 0;
            Color color1 = new Color(0.9f, 0.9f, 0.9f, 1f);
            Color color2 = new Color(0.7f, 0.7f, 0.7f, 1f);
            
            while (_isLoading && _iconImage != null && _operationId == opId)
            {
                time += Time.deltaTime * 3f;
                _iconImage.color = Color.Lerp(color1, color2, Mathf.PingPong(time, 1f));
                await UniTask.Yield();
            }
        }

        private async UniTaskVoid FadeInIcon()
        {
            if (_iconImage == null) return;
            
            float duration = 0.25f; // quarter second fade
            float elapsed = 0f;
            Color startColor = _iconImage.color;
            Color endColor = Color.white;

            while (elapsed < duration)
            {
                if (_iconImage == null) return; // safety check
                elapsed += Time.deltaTime;
                _iconImage.color = Color.Lerp(startColor, endColor, elapsed / duration);
                await UniTask.Yield();
            }
            
            if (_iconImage != null) _iconImage.color = endColor;
        }
    }
}
