using System.Collections.Generic;
using System.Linq;
using AdeebTask.Models;

namespace AdeebTask.Services.Persistence
{
    public class LocalCacheService : ILocalCacheService
    {
        // Using in-memory dictionary as the session cache.
        // It gets overwritten on boot by Firebase (Source of Truth).
        private readonly Dictionary<string, ProjectData> _cache = new Dictionary<string, ProjectData>();

        public void CacheAllProjects(List<ProjectData> projects)
        {
            _cache.Clear();
            if (projects == null) return;
            foreach (var p in projects)
            {
                _cache[p.projectId] = p;
            }
        }

        public List<ProjectCardData> GetCachedProjectCards()
        {
            return _cache.Values.Select(p => new ProjectCardData
            {
                projectId = p.projectId,
                projectName = p.projectName,
                thumbnailBase64 = p.thumbnailBase64,
                lastModifiedUtc = p.lastModifiedUtc,
                backgroundColorHex = p.backgroundColorHex
            }).ToList();
        }

        public ProjectData GetCachedProject(string projectId)
        {
            _cache.TryGetValue(projectId, out var project);
            return project;
        }

        public void UpdateCachedProject(ProjectData project)
        {
            _cache[project.projectId] = project;
        }

        public void DeleteCachedProject(string projectId)
        {
            _cache.Remove(projectId);
        }

        public void ClearCache()
        {
            _cache.Clear();
        }

        private AssetCatalogue _cachedCatalogue;

        public void CacheAssetCatalogue(AssetCatalogue catalogue)
        {
            _cachedCatalogue = catalogue;
        }

        public AssetCatalogue GetCachedAssetCatalogue()
        {
            return _cachedCatalogue;
        }
    }
}
