using Cysharp.Threading.Tasks;
using AdeebTask.Core.Events;
using AdeebTask.Core;
using AdeebTask.Services.Persistence;

namespace AdeebTask.States
{
    public class MainMenuState : IAppState
    {
        public async UniTask EnterAsync()
        {
            var eventBus = ServiceLocator.Get<IEventBus>();
            eventBus.Publish(new GlobalLoadingEvent(true, 0.8f, "Loading Projects..."));

            var firebase = ServiceLocator.Get<IFirebaseService>();
            var cache = ServiceLocator.Get<ILocalCacheService>();
            
            // 1. Fetch from Firebase
            var projects = await firebase.FetchAllProjectsAsync();
            
            // 2. Cache them locally
            cache.CacheAllProjects(projects);

            // 3. Navigate UI
            eventBus.Publish(new NavigateToMenuEvent());
            
            // 4. Hide Loader
            eventBus.Publish(new GlobalLoadingEvent(false));
        }

        public async UniTask ExitAsync()
        {
            await UniTask.CompletedTask;
        }
    }
}
