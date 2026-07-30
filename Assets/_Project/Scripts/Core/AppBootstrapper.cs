using UnityEngine;
using AdeebTask.Core.Events;
using AdeebTask.States;
using AdeebTask.Services;
using AdeebTask.Services.Persistence;
using AdeebTask.Services.Assets;
using AdeebTask.UI;
using Cysharp.Threading.Tasks;

namespace AdeebTask.Core
{
    public class AppBootstrapper : MonoBehaviour
    {
        [SerializeField] private UIManager _uiManager;

        private AppStateMachine _stateMachine;
        private FirebaseService _firebaseService;

        private void Awake()
        {
            // Register Core Services
            ServiceLocator.Register<IEventBus>(new EventBus());
            
            _stateMachine = new AppStateMachine();
            ServiceLocator.Register<AppStateMachine>(_stateMachine);

            if (_uiManager != null)
            {
                _uiManager.Initialize();
                ServiceLocator.Register<UIManager>(_uiManager);
            }
            else
            {
                Debug.LogError("[AppBootstrapper] UIManager is not assigned in the inspector!");
            }

            // Setup Persistence
            ServiceLocator.Register<ILocalCacheService>(new LocalCacheService());
            
            _firebaseService = gameObject.AddComponent<FirebaseService>();
            ServiceLocator.Register<IFirebaseService>(_firebaseService);
            
            _firebaseService.Initialize(FirebaseConfig.Json);
            
            var thumbnailService = gameObject.AddComponent<ThumbnailService>();
            ServiceLocator.Register<ThumbnailService>(thumbnailService);
            
            // Setup Assets
            ServiceLocator.Register<IAssetService>(new AssetService());
        }

        private void Start()
        {
            // Boot sequence
            _stateMachine.TransitionTo(new InitState()).Forget();
        }

        private void OnDestroy()
        {
            ServiceLocator.Reset();
        }
    }
}
