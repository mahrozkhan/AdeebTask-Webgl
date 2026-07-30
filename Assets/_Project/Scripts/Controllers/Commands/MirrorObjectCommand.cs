using UnityEngine;
using AdeebTask.Commands;
using AdeebTask.Views;
using Cysharp.Threading.Tasks;

namespace AdeebTask.Controllers.Commands
{
    public class MirrorObjectCommand : ICommand
    {
        private readonly PlacedObjectView _view;

        public MirrorObjectCommand(PlacedObjectView view)
        {
            _view = view;
        }

        public UniTask ExecuteAsync()
        {
            if (_view != null)
            {
                var scale = _view.transform.localScale;
                scale.x = -scale.x;
                _view.SetScale(scale);
            }
            return UniTask.CompletedTask;
        }

        public UniTask UndoAsync()
        {
            if (_view != null)
            {
                var scale = _view.transform.localScale;
                scale.x = -scale.x;
                _view.SetScale(scale);
            }
            return UniTask.CompletedTask;
        }
    }
}
