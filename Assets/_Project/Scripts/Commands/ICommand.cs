using Cysharp.Threading.Tasks;

namespace AdeebTask.Commands
{
    public interface ICommand
    {
        UniTask ExecuteAsync();
        UniTask UndoAsync();
    }
}
