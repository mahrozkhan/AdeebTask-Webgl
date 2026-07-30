using System.Linq;
using UnityEngine;
using AdeebTask.Models;
using AdeebTask.UI.Screens;
using AdeebTask.Core;
using AdeebTask.Core.Events;
using Cysharp.Threading.Tasks;
using AdeebTask.UI;
using AdeebTask.Services.Persistence;

namespace AdeebTask.Controllers
{
    public class PageController : MonoBehaviour
    {
        private ProjectData _currentProject;
        private int _currentPageIndex = 0;
        
        private IEventBus _eventBus;
        private UIManager _uiManager;
        private ILocalCacheService _localCache;

        private void Start()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _uiManager = ServiceLocator.Get<UIManager>();
            _localCache = ServiceLocator.Get<ILocalCacheService>();

            _eventBus.Subscribe<NavigateToEditorEvent>(HandleNavigateToEditor);
            _eventBus.Subscribe<NavigateToPlaybackEvent>(HandleNavigateToPlayback);
            _eventBus.Subscribe<NextPageRequestedEvent>(GoToNextPage);
            _eventBus.Subscribe<PrevPageRequestedEvent>(GoToPrevPage);
            _eventBus.Subscribe<AddPageRequestedEvent>(AddNewPage);
            _eventBus.Subscribe<DeletePageRequestedEvent>(HandleDeletePageRequested);
            _eventBus.Subscribe<SetBackgroundRequestedEvent>(HandleSetBackgroundRequested);
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<NavigateToEditorEvent>(HandleNavigateToEditor);
                _eventBus.Unsubscribe<NavigateToPlaybackEvent>(HandleNavigateToPlayback);
                _eventBus.Unsubscribe<NextPageRequestedEvent>(GoToNextPage);
                _eventBus.Unsubscribe<PrevPageRequestedEvent>(GoToPrevPage);
                _eventBus.Unsubscribe<AddPageRequestedEvent>(AddNewPage);
                _eventBus.Unsubscribe<DeletePageRequestedEvent>(HandleDeletePageRequested);
                _eventBus.Unsubscribe<SetBackgroundRequestedEvent>(HandleSetBackgroundRequested);
            }
        }

        private void HandleNavigateToEditor(NavigateToEditorEvent evt)
        {
            if (string.IsNullOrEmpty(evt.ProjectId)) return;
            
            _currentProject = _localCache.GetCachedProject(evt.ProjectId);
            if (_currentProject != null)
            {
                LoadPage(0).Forget();
            }
        }

        private void HandleSetBackgroundRequested(SetBackgroundRequestedEvent evt)
        {
            if (_currentProject == null || _currentProject.pages == null || _currentProject.pages.Count == 0) return;
            
            // Clear the solid color because we are applying an image
            _currentProject.pages[_currentPageIndex].backgroundColorHex = "";
            _currentProject.pages[_currentPageIndex].backgroundKey = evt.AddressableKey;
            
            // Publish event so EditorScreen can load the new image
            _eventBus.Publish(new BackgroundUpdatedEvent(evt.AddressableKey));
        }

        private void HandleNavigateToPlayback(NavigateToPlaybackEvent evt)
        {
            if (string.IsNullOrEmpty(evt.ProjectId)) return;
            
            _currentProject = _localCache.GetCachedProject(evt.ProjectId);
            if (_currentProject != null)
            {
                LoadPage(0).Forget();
            }
        }

        private async UniTaskVoid LoadPage(int index)
        {
            if (_currentProject == null || _currentProject.pages == null) return;
            
            if (_currentProject.pages.Count == 0)
            {
                _currentProject.pages.Add(new PageData 
                { 
                    pageIndex = 0, 
                    backgroundColorHex = AdeebTask.Models.CanvasColorType.White.ToString() 
                });
            }

            _currentPageIndex = Mathf.Clamp(index, 0, _currentProject.pages.Count - 1);
            var pageData = _currentProject.pages[_currentPageIndex];

            _eventBus.Publish(new PageLoadedEvent(pageData));
            
            UpdateNavUI();
            
            await UniTask.CompletedTask;
        }

        private void UpdateNavUI()
        {
            if (_currentProject != null && _currentProject.pages != null)
            {
                _eventBus.Publish(new PageNavigationStateChangedEvent(_currentPageIndex, _currentProject.pages.Count));
            }
        }

        public void GoToNextPage(NextPageRequestedEvent evt)
        {
            if (_currentPageIndex < _currentProject.pages.Count - 1)
            {
                LoadPage(_currentPageIndex + 1).Forget();
            }
        }

        public void GoToPrevPage(PrevPageRequestedEvent evt)
        {
            if (_currentPageIndex > 0)
            {
                LoadPage(_currentPageIndex - 1).Forget();
            }
        }

        public void AddNewPage(AddPageRequestedEvent evt)
        {
            var newPage = new PageData { pageIndex = _currentProject.pages.Count, backgroundColorHex = AdeebTask.Models.CanvasColorType.White.ToString() };
            _currentProject.pages.Add(newPage);
            LoadPage(newPage.pageIndex).Forget();
        }

        private void HandleDeletePageRequested(DeletePageRequestedEvent evt) => DeleteCurrentPage();

        public void DeleteCurrentPage()
        {
            if (_currentProject.pages.Count <= 1)
            {
                Debug.LogWarning("Cannot delete the only page in the project.");
                return;
            }
            
            _currentProject.pages.RemoveAt(_currentPageIndex);
            
            for (int i = 0; i < _currentProject.pages.Count; i++)
            {
                _currentProject.pages[i].pageIndex = i;
            }

            int nextIndex = Mathf.Clamp(_currentPageIndex, 0, _currentProject.pages.Count - 1);
            LoadPage(nextIndex).Forget();
        }
    }
}
