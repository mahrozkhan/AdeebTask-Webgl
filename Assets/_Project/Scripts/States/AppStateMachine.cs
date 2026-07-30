using Cysharp.Threading.Tasks;

namespace AdeebTask.States
{
    public sealed class AppStateMachine
    {
        private IAppState _current;
        public IAppState CurrentState => _current;

        public async UniTask TransitionTo(IAppState next)
        {
            if (_current != null)
            {
                await _current.ExitAsync();
            }
            
            _current = next;
            
            if (_current != null)
            {
                await _current.EnterAsync();
            }
        }
    }
}
