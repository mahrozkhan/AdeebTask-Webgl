using Cysharp.Threading.Tasks;
using AdeebTask.Core.Events;
using AdeebTask.Core;

namespace AdeebTask.States
{
    public class MainMenuState : IAppState
    {
        public async UniTask EnterAsync()
        {
            var firebase = ServiceLocator.Get<AdeebTask.Services.Persistence.IFirebaseService>();
            var cache = ServiceLocator.Get<AdeebTask.Services.Persistence.ILocalCacheService>();
            
            // 1. Fetch from Firebase
            var projects = await firebase.FetchAllProjectsAsync();
            
            // 2. Cache them locally
            cache.CacheAllProjects(projects);

            // 3. Navigate UI
            var eventBus = ServiceLocator.Get<IEventBus>();
            eventBus.Publish(new NavigateToMenuEvent());
        }

        public async UniTask ExitAsync()
        {
            await UniTask.CompletedTask;
        }
    }
}
