using UnityEngine;
using AdeebTask.Core;
using AdeebTask.Models;
using AdeebTask.Services.Persistence;
using AdeebTask.Services;
using Cysharp.Threading.Tasks;
using System;
using AdeebTask.Core.Events;
using AdeebTask.States;

namespace AdeebTask.Controllers
{
    public class SaveController : MonoBehaviour
    {
        private IFirebaseService _firebaseService;
        private ILocalCacheService _localCache;
        private ThumbnailService _thumbnailService;
        private IEventBus _eventBus;
        private AppStateMachine _stateMachine;
        
        [SerializeField] private Camera _editorCamera;

        private void Start()
        {
            _firebaseService = ServiceLocator.Get<IFirebaseService>();
            _localCache = ServiceLocator.Get<ILocalCacheService>();
            _thumbnailService = ServiceLocator.Get<ThumbnailService>(); 
            _eventBus = ServiceLocator.Get<IEventBus>();
            _stateMachine = ServiceLocator.Get<AppStateMachine>();

            _eventBus.Subscribe<SaveProjectRequestedEvent>(HandleSaveRequested);
        }

        private void OnDestroy()
        {
            if (_eventBus != null) _eventBus.Unsubscribe<SaveProjectRequestedEvent>(HandleSaveRequested);
        }

        private void HandleSaveRequested(SaveProjectRequestedEvent evt)
        {
            if (_stateMachine.CurrentState is EditorState editorState)
            {
                var projectData = _localCache.GetCachedProject(editorState.ProjectId);
                if (projectData != null)
                {
                    SaveProjectAsync(projectData, evt.NavigateToMenuAfterSave).Forget();
                }
            }
        }

        public async UniTaskVoid SaveProjectAsync(ProjectData projectData, bool navigateToMenuAfterSave = false)
        {
            // 1. Generate Thumbnail
            if (_thumbnailService != null && _editorCamera != null)
            {
                try
                {
                    projectData.thumbnailBase64 = await _thumbnailService.CaptureThumbnailBase64Async(_editorCamera);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to capture thumbnail: {e.Message}");
                }
            }

            // 2. Update version and timestamp
            projectData.version++;
            projectData.lastModifiedUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // 3. Save to Firebase
            try
            {
                await _firebaseService.SaveProjectAsync(projectData);

                // 4. Update Local Cache
                _localCache.UpdateCachedProject(projectData);
                Debug.Log("[SaveController] Project saved successfully to Firebase and Cache.");

                // 5. Navigate if requested
                if (navigateToMenuAfterSave)
                {
                    _stateMachine.TransitionTo(new MainMenuState()).Forget();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveController] Failed to save project to Firebase: {e.Message}");
            }
        }
    }
}
