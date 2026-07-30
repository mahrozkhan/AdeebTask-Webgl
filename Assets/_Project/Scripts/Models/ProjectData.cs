using System;
using System.Collections.Generic;

namespace AdeebTask.Models
{
    [Serializable]
    public class ProjectData
    {
        public string projectId;
        public string projectName;
        public string thumbnailBase64;
        public long lastModifiedUtc;
        public int version;
        public string backgroundColorHex;
        public List<PageData> pages = new List<PageData>();
    }
}
