using System;

namespace AdeebTask.Models
{
    [Serializable]
    public class PlacedObjectData
    {
        public string objectId;
        public string assetKey;
        public float posX;
        public float posY;
        public float scaleX;
        public float scaleY;
        public float rotation;
        public int sortingOrder;
    }
}
