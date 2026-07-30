using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using AdeebTask.Models;

namespace AdeebTask.Services.Persistence
{
    public interface IFirebaseService
    {
        void Initialize(string configJson);
        UniTask<List<ProjectData>> FetchAllProjectsAsync();
        UniTask<AssetCatalogue> FetchAssetCatalogueAsync();
        UniTask SaveProjectAsync(ProjectData project);
        UniTask DeleteProjectAsync(string projectId);
    }
}
