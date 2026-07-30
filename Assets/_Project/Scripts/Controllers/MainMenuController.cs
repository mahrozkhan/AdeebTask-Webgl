using UnityEngine;
using AdeebTask.Core;
using AdeebTask.Core.Events;
using AdeebTask.Services.Persistence;
using AdeebTask.UI;
using AdeebTask.UI.Screens;
using AdeebTask.States;
using Cysharp.Threading.Tasks;

namespace AdeebTask.Controllers
{
    public class MainMenuController : MonoBehaviour
    {
        private IEventBus _eventBus;
        private ILocalCacheService _localCache;
        private UIManager _uiManager;
        private AppStateMachine _stateMachine;

        private void Start()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _localCache = ServiceLocator.Get<ILocalCacheService>();
            _uiManager = ServiceLocator.Get<UIManager>();
            _stateMachine = ServiceLocator.Get<AppStateMachine>();

            _eventBus.Subscribe<CreateNewProjectRequestedEvent>(HandleCreateNewProject);
            _eventBus.Subscribe<OpenProjectRequestedEvent>(HandleOpenProject);
            _eventBus.Subscribe<NavigateToMenuEvent>(OnNavigateToMenu);
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<CreateNewProjectRequestedEvent>(HandleCreateNewProject);
                _eventBus.Unsubscribe<OpenProjectRequestedEvent>(HandleOpenProject);
                _eventBus.Unsubscribe<NavigateToMenuEvent>(OnNavigateToMenu);
            }
        }

        private void OnNavigateToMenu(NavigateToMenuEvent evt)
        {
            _uiManager.Show<CardListScreen>();
            RefreshProjectList();
        }

        private void RefreshProjectList()
        {
            var projects = _localCache.GetCachedProjectCards();
            var screen = _uiManager.GetScreen<CardListScreen>();
            if (screen != null) screen.DisplayProjects(projects);
        }

        private void HandleCreateNewProject(CreateNewProjectRequestedEvent evt)
        {
            _stateMachine.TransitionTo(new ProjectSetupState()).Forget();
        }

        private void HandleOpenProject(OpenProjectRequestedEvent evt)
        {
            _stateMachine.TransitionTo(new EditorState(evt.ProjectId)).Forget();
        }
    }
}
