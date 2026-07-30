using System;
using System.Collections.Generic;

namespace AdeebTask.Models
{
    [Serializable]
    public class AssetCatalogue
    {
        public List<AssetCategory> categories = new List<AssetCategory>();
    }

    [Serializable]
    public class AssetCategory
    {
        public string id;
        public string name;
        public string iconAddressableKey;
        public List<AssetItem> items = new List<AssetItem>();
    }

    [Serializable]
    public class AssetItem
    {
        public string id;
        public string name;
        public string addressableKey;
    }
}
