using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using AdeebTask.Core;
using AdeebTask.Services.Persistence;
using AdeebTask.Core.Events;
using AdeebTask.UI;
using AdeebTask.UI.Screens;

namespace AdeebTask.States
{
    public class InitState : IAppState
    {
        public async UniTask EnterAsync()
        {
            var ui = ServiceLocator.Get<UIManager>();
            var screen = ui.Show<InitScreen>();
            if (screen != null) screen.UpdateProgress(0f, "Fetching Catalogue...");

            var firebase = ServiceLocator.Get<IFirebaseService>();
            var cache = ServiceLocator.Get<ILocalCacheService>();

            // 1. Fetch JSON Catalogue
            var catalogue = await firebase.FetchAssetCatalogueAsync();
            cache.CacheAssetCatalogue(catalogue);

            // 2. Extract Addressable Keys
            List<object> keysToDownload = new List<object>();
            if (catalogue != null && catalogue.categories != null)
            {
                foreach (var cat in catalogue.categories)
                {
                    if (cat.items != null)
                    {
                        foreach (var item in cat.items)
                        {
                            if (!string.IsNullOrEmpty(item.addressableKey))
                            {
                                keysToDownload.Add(item.addressableKey);
                            }
                        }
                    }
                }
            }

            // 3. Pre-Download Addressables (Using new API)
            if (keysToDownload.Count > 0)
            {
                if (screen != null) screen.UpdateProgress(0.1f, "Finding remote assets...");
                
                // Get Locations first (modern non-deprecated way)
                var locationsHandle = Addressables.LoadResourceLocationsAsync((IEnumerable<object>)keysToDownload, Addressables.MergeMode.Union);
                var locations = await locationsHandle.ToUniTask();

                if (locations.Count > 0)
                {
                    var sizeHandle = Addressables.GetDownloadSizeAsync(locations);
                    long totalSize = await sizeHandle.ToUniTask();

                    if (totalSize > 0)
                    {
                        var downloadHandle = Addressables.DownloadDependenciesAsync(locations);
                        while (!downloadHandle.IsDone)
                        {
                            if (screen != null)
                            {
                                float p = downloadHandle.PercentComplete;
                                screen.UpdateProgress(0.1f + (p * 0.9f), $"Downloading Assets... {UnityEngine.Mathf.RoundToInt(p * 100)}%");
                            }
                            await UniTask.Yield();
                        }
                    }
                }
            }

            if (screen != null) screen.UpdateProgress(1f, "Ready!");
            await UniTask.Delay(300); // Slight delay so user sees "Ready!"

            // 4. Proceed to Main Menu
            ServiceLocator.Get<AppStateMachine>().TransitionTo(new MainMenuState()).Forget();
        }

        public async UniTask ExitAsync()
        {
            await UniTask.CompletedTask;
        }
    }
}
