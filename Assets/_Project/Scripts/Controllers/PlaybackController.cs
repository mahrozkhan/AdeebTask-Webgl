using UnityEngine;
using AdeebTask.Core;
using AdeebTask.Core.Events;
using AdeebTask.Services.Persistence;
using AdeebTask.UI;
using Cysharp.Threading.Tasks;

namespace AdeebTask.Controllers
{
    public class PlaybackController : MonoBehaviour
    {
        private IEventBus _eventBus;
        private UIManager _uiManager;
        private ILocalCacheService _localCache;

        private void Start()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _uiManager = ServiceLocator.Get<UIManager>();
            _localCache = ServiceLocator.Get<ILocalCacheService>();

            _eventBus.Subscribe<NavigateToPlaybackEvent>(OnNavigateToPlayback);
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<NavigateToPlaybackEvent>(OnNavigateToPlayback);
            }
        }

        private void OnNavigateToPlayback(NavigateToPlaybackEvent evt)
        {
            // Show a simplified playback UI (no toolboxes)
            // _uiManager.Show<PlaybackScreen>(); 
        }
    }
}
