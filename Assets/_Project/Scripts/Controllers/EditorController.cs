using UnityEngine;
using AdeebTask.Core;
using AdeebTask.Core.Events;
using AdeebTask.UI;
using AdeebTask.UI.Screens;
using AdeebTask.Services.Assets;
using AdeebTask.Models;
using Cysharp.Threading.Tasks;
using AdeebTask.Services.Persistence;
using AdeebTask.States;
using System.Collections.Generic;

namespace AdeebTask.Controllers
{
    public class EditorController : MonoBehaviour
    {
        private IEventBus _eventBus;
        private ILocalCacheService _localCache;
        private AppStateMachine _stateMachine;
        private bool _isNewProject = false;
        private string _currentProjectId;

        private void Start()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _localCache = ServiceLocator.Get<ILocalCacheService>();
            _stateMachine = ServiceLocator.Get<AppStateMachine>();

            _eventBus.Subscribe<NavigateToEditorEvent>(OnNavigateToEditor);
            _eventBus.Subscribe<ProjectSetupConfirmedEvent>(HandleSetupConfirmed);
            _eventBus.Subscribe<ProjectSetupCancelledEvent>(HandleSetupCancel);
            _eventBus.Subscribe<EditorQuitRequestedEvent>(HandleEditorQuitRequested);
            _eventBus.Subscribe<ConfirmationPopupResponseEvent>(HandlePopupResponse);
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<NavigateToEditorEvent>(OnNavigateToEditor);
                _eventBus.Unsubscribe<ProjectSetupConfirmedEvent>(HandleSetupConfirmed);
                _eventBus.Unsubscribe<ProjectSetupCancelledEvent>(HandleSetupCancel);
                _eventBus.Unsubscribe<EditorQuitRequestedEvent>(HandleEditorQuitRequested);
                _eventBus.Unsubscribe<ConfirmationPopupResponseEvent>(HandlePopupResponse);
            }
        }

        private void HandleSetupConfirmed(ProjectSetupConfirmedEvent evt)
        {
            string newId = System.Guid.NewGuid().ToString();
            string themeString = CanvasColorType.White.ToString();

            int projectCount = _localCache.GetCachedProjectCards().Count;
            string generatedName = $"Project {projectCount + 1}";
            
            var projectData = new ProjectData 
            { 
                projectId = newId, 
                projectName = generatedName, 
                lastModifiedUtc = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                version = 1,
                backgroundColorHex = themeString,
                pages = new List<PageData>
                {
                    new PageData
                    {
                        pageIndex = 0,
                        backgroundColorHex = themeString,
                        backgroundKey = evt.BackgroundKey,
                        objects = new List<PlacedObjectData>()
                    }
                }
            };
            
            _localCache.UpdateCachedProject(projectData);
            
            _isNewProject = true;
            _currentProjectId = newId;

            _stateMachine.TransitionTo(new EditorState(newId, true)).Forget();
        }

        private void HandleSetupCancel(ProjectSetupCancelledEvent evt)
        {
            _stateMachine.TransitionTo(new MainMenuState()).Forget();
        }

        private async void OnNavigateToEditor(NavigateToEditorEvent evt)
        {
            // Overlay the InitScreen manually using EventBus so it isn't hidden by UIManager
            _eventBus.Publish(new GlobalLoadingEvent(true, 0.9f, "Loading Workspace..."));
            
            _eventBus.Publish(new EditorProjectLoadedEvent());

            if (!string.IsNullOrEmpty(evt.ProjectId))
            {
                var project = _localCache.GetCachedProject(evt.ProjectId);

                if (project != null)
                {
                    _isNewProject = evt.IsNewProject;
                    _currentProjectId = evt.ProjectId;

                    _eventBus.Publish(new EditorModeChangedEvent(!_isNewProject));
                }
            }

            // Give Addressables 1 second to fetch assets and apply to the canvas before dropping the curtain
            await UniTask.Delay(1000);
            _eventBus.Publish(new GlobalLoadingEvent(false));
        }

        private void HandleEditorQuitRequested(EditorQuitRequestedEvent evt)
        {
            if (_isNewProject)
            {
                _eventBus.Publish(new ShowConfirmationPopupEvent("EditorQuit", PopupType.StandardConfirm, "Quit Editor", "Do you want to save your new project?"));
            }
            else
            {
                _stateMachine.TransitionTo(new MainMenuState()).Forget();
            }
        }

        private void HandlePopupResponse(ConfirmationPopupResponseEvent evt)
        {
            if (evt.PopupId == "EditorQuit")
            {
                if (evt.IsConfirmed)
                {
                    _isNewProject = false;
                    _eventBus.Publish(new SaveProjectRequestedEvent(true));
                }
                else
                {
                    _isNewProject = false;
                    if (!string.IsNullOrEmpty(_currentProjectId))
                    {
                        _localCache.DeleteCachedProject(_currentProjectId);
                    }
                    _stateMachine.TransitionTo(new MainMenuState()).Forget();
                }
            }
        }
    }
}
