using Cysharp.Threading.Tasks;

namespace AdeebTask.States
{
    public interface IAppState
    {
        UniTask EnterAsync();
        UniTask ExitAsync();
    }
}
