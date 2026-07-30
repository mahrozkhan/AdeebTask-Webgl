using System.Collections.Generic;
using AdeebTask.Models;

namespace AdeebTask.Services.Persistence
{
    public interface ILocalCacheService
    {
        void CacheAllProjects(List<ProjectData> projects);
        List<ProjectCardData> GetCachedProjectCards();
        ProjectData GetCachedProject(string projectId);
        void UpdateCachedProject(ProjectData project);
        void DeleteCachedProject(string projectId);
        void ClearCache();
        
        void CacheAssetCatalogue(AssetCatalogue catalogue);
        AssetCatalogue GetCachedAssetCatalogue();
    }
}
