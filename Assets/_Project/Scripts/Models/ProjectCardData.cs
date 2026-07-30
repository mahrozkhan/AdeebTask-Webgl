using System;

namespace AdeebTask.Models
{
    [Serializable]
    public class ProjectCardData
    {
        public string projectId;
        public string projectName;
        public string thumbnailBase64;
        public long lastModifiedUtc;
        public string backgroundColorHex;
    }
}
