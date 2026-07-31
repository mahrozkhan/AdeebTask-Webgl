using Cysharp.Threading.Tasks;
using AdeebTask.Core.Events;
using AdeebTask.Core;
using AdeebTask.States;
using ContentDiscovery.Services;
using ContentDiscovery.Events;
using AdeebTask.UI;
using ContentDiscovery.UI;

namespace ContentDiscovery.States
{
    public class ContentDiscoveryState : IAppState
    {
        public async UniTask EnterAsync()
        {
            var eventBus = ServiceLocator.Get<IEventBus>();
            eventBus.Publish(new GlobalLoadingEvent(true, 0.8f, "Loading Content Library..."));

            var dataService = ServiceLocator.Get<IDataContentService>();
            
            // 1. Fetch data from the decoupled backend
            bool success = await dataService.FetchDataAsync();

            // 2. Hide Loader
            eventBus.Publish(new GlobalLoadingEvent(false));
            
            if (success)
            {
                // 3. Command UI to swap screens
                var uiManager = ServiceLocator.Get<UIManager>();
                uiManager.Show<SearchScreen>();
            }
            else
            {
                // Show Error Popup and immediately exit back to Task 1
                eventBus.Publish(new ShowConfirmationPopupEvent(
                    "error_task2", 
                    PopupType.ConnectionError,
                    "Connection Error", 
                    "Failed to download the Library Database. Please check your internet connection."
                ));
                
                if (ServiceLocator.TryGet<AppStateMachine>(out var stateMachine))
                {
                    stateMachine.TransitionTo(new MainMenuState()).Forget();
                }
            }
        }

        public async UniTask ExitAsync()
        {
            // Discard memory when leaving Task 2 to free up WebGL Heap for Task 1
            if (ServiceLocator.TryGet<ContentSearchService>(out var searchService))
            {
                searchService.Clear();
            }

            await UniTask.CompletedTask;
        }
    }
}
