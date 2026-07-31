using System.Collections.Generic;
using ContentDiscovery.Models;
using ContentDiscovery.Events;
using AdeebTask.Core;
using AdeebTask.Core.Events;

namespace ContentDiscovery.Services
{
    public class ContentSearchService
    {
        private ContentSearchIndex _searchIndex;
        private IEventBus _eventBus;

        public ContentSearchService()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _eventBus.Subscribe<RawContentFetchedEvent>(OnRawContentFetched);
            _searchIndex = new ContentSearchIndex();
        }

        public void Destroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<RawContentFetchedEvent>(OnRawContentFetched);
            }
            Clear();
        }

        private void OnRawContentFetched(RawContentFetchedEvent evt)
        {
            foreach (var item in evt.Items)
            {
                _searchIndex.Add(item);
            }
        }

        public List<ContentItem> Search(string query)
        {
            return _searchIndex.Search(query);
        }

        public void Clear()
        {
            // Explicitly clear all internal dictionaries and lists as per best practices
            _searchIndex.Clear();
        }
    }
}
