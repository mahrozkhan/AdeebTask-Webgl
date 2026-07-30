using Cysharp.Threading.Tasks;
using AdeebTask.Core.Events;
using AdeebTask.Core;

namespace AdeebTask.States
{
    public class PlaybackState : IAppState
    {
        private readonly string _projectId;

        public PlaybackState(string projectId)
        {
            _projectId = projectId;
        }

        public async UniTask EnterAsync()
        {
            var eventBus = ServiceLocator.Get<IEventBus>();
            eventBus.Publish(new NavigateToPlaybackEvent(_projectId));
            
            await UniTask.CompletedTask;
        }

        public async UniTask ExitAsync()
        {
            var assetService = ServiceLocator.Get<Services.Assets.IAssetService>();
            assetService.ReleaseAll(); 
            
            await UniTask.CompletedTask;
        }
    }
}
