using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AdeebTask.Models;
using AdeebTask.Core;
using Cysharp.Threading.Tasks;
using AdeebTask.Services.Assets;

namespace AdeebTask.UI.Screens
{
    public class AssetItemView : MonoBehaviour
    {
        public event Action<string> OnItemClicked;

        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private Button _itemButton;
        [SerializeField] private Image _iconImage;

        private string _addressableKey;
        private IAssetService _assetService;
        private AssetHandle<Sprite> _spriteHandle;
        private bool _isLoading = false;
        private int _operationId = 0;

        private void Awake()
        {
            if (_itemButton != null)
                _itemButton.onClick.AddListener(() => OnItemClicked?.Invoke(_addressableKey));
        }

        private void OnDestroy()
        {
            if (_itemButton != null)
                _itemButton.onClick.RemoveAllListeners();
                
            ClearMemory();
        }

        public void ClearMemory()
        {
            if (_spriteHandle != null && _assetService != null)
            {
                _assetService.Release(_spriteHandle.Key);
                _spriteHandle = null;
            }
            if (_iconImage != null)
            {
                _iconImage.sprite = null;
            }
        }

        public void Setup(AssetItem data, IAssetService assetService)
        {
            _assetService = assetService;
            _addressableKey = data.addressableKey;
            
            if (_nameText != null) 
            {
                _nameText.gameObject.SetActive(false); // Hide text if present
            }
            
            if (_iconImage != null)
            {
                _iconImage.sprite = null;
                _iconImage.enabled = true;
                _isLoading = true;
                _operationId++;
                PulseLoader(_operationId).Forget();
            }

            LoadIconAsync(data.addressableKey, _operationId).Forget();
        }

        private async UniTaskVoid LoadIconAsync(string key, int opId)
        {
            if (string.IsNullOrEmpty(key) || _iconImage == null || _assetService == null) return;
            
            if (_spriteHandle != null)
            {
                _assetService.Release(_spriteHandle.Key);
                _spriteHandle = null;
            }

            _spriteHandle = await _assetService.AcquireAsync<Sprite>(key);
            
            if (_operationId == opId)
            {
                _isLoading = false;
            }
            
            if (_operationId != opId) return; // Stale request, ignore!
            
            // Safety check if object was destroyed during async load
            if (this == null || _iconImage == null)
            {
                if (_spriteHandle != null) _assetService.Release(_spriteHandle.Key);
                return;
            }

            if (_spriteHandle != null && _spriteHandle.Asset != null)
            {
                _iconImage.sprite = _spriteHandle.Asset;
                _iconImage.color = new Color(1, 1, 1, 0); // Start transparent for fade in
                _iconImage.enabled = true;
                FadeInIcon().Forget();
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
