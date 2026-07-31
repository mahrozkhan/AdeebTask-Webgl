using UnityEngine;
using AdeebTask.Core;
using AdeebTask.Core.Events;
using AdeebTask.Services.Persistence;
using AdeebTask.UI.Screens;
using AdeebTask.States;
using ContentDiscovery.States;
using Cysharp.Threading.Tasks;
using AdeebTask.UI;

namespace AdeebTask.Controllers
{
    public class MainMenuController : MonoBehaviour
    {
        private IEventBus _eventBus;
        private ILocalCacheService _localCache;
        private AppStateMachine _stateMachine;

        private void Start()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _localCache = ServiceLocator.Get<ILocalCacheService>();
            _stateMachine = ServiceLocator.Get<AppStateMachine>();

            _eventBus.Subscribe<CreateNewProjectRequestedEvent>(HandleCreateNewProject);
            _eventBus.Subscribe<LaunchTask2RequestedEvent>(HandleLaunchTask2);
            _eventBus.Subscribe<OpenProjectRequestedEvent>(HandleOpenProject);
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<CreateNewProjectRequestedEvent>(HandleCreateNewProject);
                _eventBus.Unsubscribe<LaunchTask2RequestedEvent>(HandleLaunchTask2);
                _eventBus.Unsubscribe<OpenProjectRequestedEvent>(HandleOpenProject);
            }
        }

        private void HandleCreateNewProject(CreateNewProjectRequestedEvent evt)
        {
            _stateMachine.TransitionTo(new ProjectSetupState()).Forget();
        }

        private void HandleLaunchTask2(LaunchTask2RequestedEvent evt)
        {
            _stateMachine.TransitionTo(new ContentDiscoveryState()).Forget();
        }

        private void HandleOpenProject(OpenProjectRequestedEvent evt)
        {
            _stateMachine.TransitionTo(new EditorState(evt.ProjectId)).Forget();
        }
    }
}
