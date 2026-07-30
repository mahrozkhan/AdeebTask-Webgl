using UnityEngine;
using Cysharp.Threading.Tasks;
using AdeebTask.Core;

namespace AdeebTask.States
{
    public class ProjectSetupState : IAppState
    {
        public async UniTask EnterAsync()
        {
            ServiceLocator.Get<Core.Events.IEventBus>().Publish(new Core.Events.NavigateToProjectSetupEvent());
            await UniTask.CompletedTask;
        }

        public async UniTask ExitAsync()
        {
            await UniTask.CompletedTask;
        }
    }
}
