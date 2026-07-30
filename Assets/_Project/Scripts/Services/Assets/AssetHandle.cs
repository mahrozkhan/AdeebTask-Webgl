using UnityEngine;

namespace AdeebTask.Services.Assets
{
    public interface IAssetHandle
    {
        string Key { get; }
        int RefCount { get; set; }
        void UnloadAddressable();
    }

    public class AssetHandle<T> : IAssetHandle where T : Object
    {
        public string Key { get; }
        public T Asset { get; }
        public int RefCount { get; set; }

        public AssetHandle(string key, T asset)
        {
            Key = key;
            Asset = asset;
            RefCount = 1;
        }

        public void UnloadAddressable()
        {
            if (Asset != null)
            {
                UnityEngine.AddressableAssets.Addressables.Release(Asset);
            }
        }
    }
}
