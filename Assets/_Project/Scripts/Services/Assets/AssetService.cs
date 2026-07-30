using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

namespace AdeebTask.Services.Assets
{
    public class AssetService : IAssetService
    {
        private readonly Dictionary<string, IAssetHandle> _handles = new Dictionary<string, IAssetHandle>();

        public async UniTask<AssetHandle<T>> AcquireAsync<T>(string key) where T : Object
        {
            if (string.IsNullOrEmpty(key)) return null;

            // If already loaded/loading, just increment ref count and return
            if (_handles.TryGetValue(key, out var existingHandle))
            {
                existingHandle.RefCount++;
                return (AssetHandle<T>)existingHandle;
            }

            // Load new asset manually to avoid UniTask/Addressables sync bugs
            var loadHandle = Addressables.LoadAssetAsync<T>(key);
            while (!loadHandle.IsDone)
            {
                await UniTask.Yield();
            }

            if (loadHandle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Failed)
            {
                throw new System.Exception($"Failed to load Addressable asset for key: {key}");
            }

            var newHandle = new AssetHandle<T>(key, loadHandle.Result);
            _handles[key] = newHandle;
            
            return newHandle;
        }

        public void Release(string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            if (_handles.TryGetValue(key, out var handle))
            {
                handle.RefCount--;
                if (handle.RefCount <= 0)
                {
                    handle.UnloadAddressable();
                    _handles.Remove(key);
                }
            }
        }

        public void ReleaseAll()
        {
            var keys = _handles.Keys.ToList();
            foreach (var k in keys)
            {
                _handles[k].UnloadAddressable();
            }
            _handles.Clear();
        }
    }
}
