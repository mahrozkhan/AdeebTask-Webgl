using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AdeebTask.Core;
using AdeebTask.Core.Events;
using AdeebTask.UI;
using ContentDiscovery.Events;
using ContentDiscovery.UI.Virtualization;
using AdeebTask.States;

namespace ContentDiscovery.UI
{
    public class SearchScreen : AppScreen
    {
        [Header("Search Input")]
        [SerializeField] private TMP_InputField _searchInput;
        [SerializeField] private Button _searchButton;
        [SerializeField] private Button _backButton;

        [Header("Virtualization")]
        [SerializeField] private ContentVirtualScroller _scroller;

        private IEventBus _eventBus;

        public override void Initialize()
        {
            base.Initialize();
            _eventBus = ServiceLocator.Get<IEventBus>();
            _eventBus.Subscribe<SearchResultsUpdatedEvent>(OnResultsUpdated);

            if (_searchButton != null)
            {
                _searchButton.onClick.AddListener(OnSearchClicked);
            }

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackClicked);
            }
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<SearchResultsUpdatedEvent>(OnResultsUpdated);
            }
        }

        private void OnSearchClicked()
        {
            if (_searchInput != null)
            {
                _eventBus.Publish(new SearchRequestedEvent(_searchInput.text));
            }
        }

        private void OnBackClicked()
        {
            if (_eventBus != null)
            {
                _eventBus.Publish(new NavigateBackRequestedEvent());
            }
        }

        private void OnResultsUpdated(SearchResultsUpdatedEvent evt)
        {
            if (_scroller != null)
            {
                _scroller.SetData(evt.Results);
            }
        }
    }
}
