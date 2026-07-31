using System;

namespace ContentDiscovery.Models
{
    public class ContentItem
    {
        public string Id { get; set; }
        
        public string ContentName { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
        public DateTime? Date { get; set; }

        // Cached lowercase fields for zero-allocation search
        public string NameLower { get; set; }
        public string AuthorLower { get; set; }

        public bool ParseSucceeded { get; set; }
    }
}
