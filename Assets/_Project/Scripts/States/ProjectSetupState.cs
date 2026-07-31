using UnityEngine;
using Cysharp.Threading.Tasks;
using AdeebTask.Core;
using AdeebTask.UI;
using AdeebTask.UI.Screens;

namespace AdeebTask.States
{
    public class ProjectSetupState : IAppState
    {
        public async UniTask EnterAsync()
        {
            var uiManager = ServiceLocator.Get<UIManager>();
            uiManager.Show<ProjectSetupScreen>();
            await UniTask.CompletedTask;
        }

        public async UniTask ExitAsync()
        {
            await UniTask.CompletedTask;
        }
    }
}
