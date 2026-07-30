using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Cysharp.Threading.Tasks;
using AdeebTask.Models;

namespace AdeebTask.Services.Persistence
{
    public class FirebaseService : MonoBehaviour, IFirebaseService
    {
        [DllImport("__Internal")] private static extern void JS_FirebaseInit(string configJson);
        [DllImport("__Internal")] private static extern void JS_FirebaseSaveProject(string json, string goName, string callback);
        [DllImport("__Internal")] private static extern void JS_FirebaseLoadProjectList(string goName, string callback);
        [DllImport("__Internal")] private static extern void JS_FirebaseDeleteProject(string projectId, string goName, string callback);
        [DllImport("__Internal")] private static extern void JS_FirebaseLoadAssetCatalogue(string goName, string callback);

        private UniTaskCompletionSource<string> _pendingCallback;
        private static List<ProjectData> _mockDatabase = null;

        public void Initialize(string configJson)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            JS_FirebaseInit(configJson);
#else
            Debug.Log($"[FirebaseService] Mock Init with config: {configJson}");
#endif
        }

        public void OnFirebaseCallback(string result)
        {
            _pendingCallback?.TrySetResult(result);
        }

        public async UniTask<List<ProjectData>> FetchAllProjectsAsync()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            _pendingCallback = new UniTaskCompletionSource<string>();
            JS_FirebaseLoadProjectList(gameObject.name, nameof(OnFirebaseCallback));
            var result = await _pendingCallback.Task;
            if (result.StartsWith("error:"))
            {
                Debug.LogError($"Firebase Load Failed: {result}");
                return new List<ProjectData>();
            }
            var wrapper = JsonUtility.FromJson<ProjectListWrapper>("{\"items\":" + result + "}");
            return wrapper.items ?? new List<ProjectData>();
#else
            Debug.Log("[FirebaseService] Mock FetchAllProjectsAsync");
            await UniTask.Delay(500);
            
            if (_mockDatabase == null)
            {
                _mockDatabase = new List<ProjectData>
                {
                    new ProjectData { projectId = "mock_1", projectName = "My Awesome Canvas", lastModifiedUtc = 1704067200, thumbnailBase64 = "" },
                    new ProjectData { projectId = "mock_2", projectName = "Summer Presentation", lastModifiedUtc = 1706745600, thumbnailBase64 = "" }
                };
            }
            
            return new List<ProjectData>(_mockDatabase);
#endif
        }

        public async UniTask SaveProjectAsync(ProjectData project)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            _pendingCallback = new UniTaskCompletionSource<string>();
            var json = JsonUtility.ToJson(project);
            JS_FirebaseSaveProject(json, gameObject.name, nameof(OnFirebaseCallback));
            var result = await _pendingCallback.Task;
            if (result.StartsWith("error:"))
                Debug.LogError($"Firebase Save Failed: {result}");
#else
            Debug.Log($"[FirebaseService] Mock SaveProjectAsync: {project.projectId}");
            await UniTask.Delay(500);
            
            if (_mockDatabase == null)
            {
                await FetchAllProjectsAsync();
            }
            
            int index = _mockDatabase.FindIndex(p => p.projectId == project.projectId);
            if (index >= 0)
            {
                _mockDatabase[index] = project;
            }
            else
            {
                _mockDatabase.Add(project);
            }
#endif
        }

        public async UniTask DeleteProjectAsync(string projectId)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            _pendingCallback = new UniTaskCompletionSource<string>();
            JS_FirebaseDeleteProject(projectId, gameObject.name, nameof(OnFirebaseCallback));
            var result = await _pendingCallback.Task;
            if (result.StartsWith("error:"))
                Debug.LogError($"Firebase Delete Failed: {result}");
#else
            Debug.Log($"[FirebaseService] Mock DeleteProjectAsync: {projectId}");
            await UniTask.Delay(500);
            
            if (_mockDatabase != null)
            {
                _mockDatabase.RemoveAll(p => p.projectId == projectId);
            }
#endif
        }

        public async UniTask<AssetCatalogue> FetchAssetCatalogueAsync()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            _pendingCallback = new UniTaskCompletionSource<string>();
            JS_FirebaseLoadAssetCatalogue(gameObject.name, nameof(OnFirebaseCallback));
            var result = await _pendingCallback.Task;
            if (result.StartsWith("error:"))
            {
                Debug.LogError($"Firebase Catalogue Failed: {result}");
                return new AssetCatalogue();
            }
            var catalogue = JsonUtility.FromJson<AssetCatalogue>(result);
            return catalogue ?? new AssetCatalogue();
#else
            Debug.Log("[FirebaseService] Mock FetchAssetCatalogueAsync");
            await UniTask.Delay(300); // Simulate network

#if UNITY_EDITOR
            string jsonPath = System.IO.Path.Combine(Application.dataPath, "_Project/Config/asset_catalogue.json");
            if (System.IO.File.Exists(jsonPath))
            {
                string json = System.IO.File.ReadAllText(jsonPath);
                var parsedCatalogue = JsonUtility.FromJson<AssetCatalogue>(json);
                if (parsedCatalogue != null)
                {
                    return parsedCatalogue;
                }
            }
#endif
            return new AssetCatalogue();
#endif
        }
    }
}
