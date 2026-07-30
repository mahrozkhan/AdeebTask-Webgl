using System;
using System.Collections.Generic;

namespace AdeebTask.Models
{
    [Serializable]
    public class PageData
    {
        public int pageIndex;
        public string backgroundKey; // Addressable key if image
        public string backgroundColorHex; // Hex string (e.g. "#FF0000") if solid color
        public List<PlacedObjectData> objects = new List<PlacedObjectData>();
    }
}
