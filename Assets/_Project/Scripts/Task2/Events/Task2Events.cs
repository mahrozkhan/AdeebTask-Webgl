using System.Collections.Generic;
using ContentDiscovery.Models;

namespace ContentDiscovery.Events
{
    public struct LibraryDataLoadedEvent { }
    
    public struct LibraryDataLoadErrorEvent 
    {
        public string ErrorMessage;
        public LibraryDataLoadErrorEvent(string errorMessage) 
        {
            ErrorMessage = errorMessage;
        }
    }

    public struct SearchRequestedEvent 
    {
        public string Query;
        public SearchRequestedEvent(string query) 
        {
            Query = query;
        }
    }

    public struct SearchResultsUpdatedEvent 
    {
        public List<ContentItem> Results;
        public SearchResultsUpdatedEvent(List<ContentItem> results) 
        {
            Results = results;
        }
    }

    public struct RawContentFetchedEvent 
    {
        public List<ContentItem> Items;
        public RawContentFetchedEvent(List<ContentItem> items) 
        {
            Items = items;
        }
    }

    public struct NavigateToContentDiscoveryEvent { }

    public struct NavigateBackRequestedEvent { }
}
