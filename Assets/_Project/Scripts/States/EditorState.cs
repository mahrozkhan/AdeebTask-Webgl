using Cysharp.Threading.Tasks;
using AdeebTask.Core.Events;
using AdeebTask.Core;
using AdeebTask.UI;
using AdeebTask.UI.Screens;

namespace AdeebTask.States
{
    public class EditorState : IAppState
    {
        public string ProjectId { get; private set; }
        public bool IsNewProject { get; private set; }

        public EditorState(string projectId = null, bool isNewProject = false)
        {
            ProjectId = projectId;
            IsNewProject = isNewProject;
        }

        public async UniTask EnterAsync()
        {
            var uiManager = ServiceLocator.Get<UIManager>();
            uiManager.Show<EditorScreen>();

            var eventBus = ServiceLocator.Get<IEventBus>();
            eventBus.Publish(new NavigateToEditorEvent(ProjectId, IsNewProject));
            
            await UniTask.CompletedTask;
        }

        public async UniTask ExitAsync()
        {
            // The FSM guarantees this is called when leaving the editor.
            // This is the absolute failsafe for WebGL memory management.
            var assetService = ServiceLocator.Get<Services.Assets.IAssetService>();
            assetService.ReleaseAll(); 
            
            var eventBus = ServiceLocator.Get<IEventBus>();
            eventBus.Publish(new NavigateToMenuEvent());
            
            await UniTask.CompletedTask;
        }
    }
}
