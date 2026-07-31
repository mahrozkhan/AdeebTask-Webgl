using UnityEngine;
using AdeebTask.Core;
using AdeebTask.Core.Events;
using ContentDiscovery.Events;
using ContentDiscovery.Services;
using AdeebTask.States;
using Cysharp.Threading.Tasks;

namespace ContentDiscovery.Controllers
{
    public class SearchController : MonoBehaviour
    {
        private IEventBus _eventBus;
        private ContentSearchService _searchService;

        private void Start()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _eventBus.Subscribe<SearchRequestedEvent>(OnSearchRequested);
            _eventBus.Subscribe<NavigateBackRequestedEvent>(OnNavigateBackRequested);
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<SearchRequestedEvent>(OnSearchRequested);
                _eventBus.Unsubscribe<NavigateBackRequestedEvent>(OnNavigateBackRequested);
            }
        }

        private void OnNavigateBackRequested(NavigateBackRequestedEvent evt)
        {
            if (ServiceLocator.TryGet<AppStateMachine>(out var stateMachine))
            {
                stateMachine.TransitionTo(new MainMenuState()).Forget();
            }
            else
            {
                Debug.LogError("SearchController: AppStateMachine not found in ServiceLocator.");
            }
        }

        private void OnSearchRequested(SearchRequestedEvent evt)
        {
            if (_searchService == null)
            {
                if (!ServiceLocator.TryGet(out _searchService))
                {
                    Debug.LogError("SearchController: ContentSearchService is not registered!");
                    return;
                }
            }

            // Execute the O(1) Trie search decoupled from Firebase
            var results = _searchService.Search(evt.Query);
            
            // Publish the results back to the View layer
            _eventBus.Publish(new SearchResultsUpdatedEvent(results));
        }
    }
}
