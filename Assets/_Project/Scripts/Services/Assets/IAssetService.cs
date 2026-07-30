using UnityEngine;
using Cysharp.Threading.Tasks;

namespace AdeebTask.Services.Assets
{
    public interface IAssetService
    {
        UniTask<AssetHandle<T>> AcquireAsync<T>(string key) where T : Object;
        void Release(string key);
        void ReleaseAll();
    }
}
