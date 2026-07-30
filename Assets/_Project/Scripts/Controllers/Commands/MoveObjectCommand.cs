using UnityEngine;
using AdeebTask.Commands;
using AdeebTask.Views;
using Cysharp.Threading.Tasks;

namespace AdeebTask.Controllers.Commands
{
    public class MoveObjectCommand : ICommand
    {
        private readonly PlacedObjectView _view;
        private readonly Vector2 _oldPosition;
        private readonly Vector2 _newPosition;
        private readonly AdeebTask.Models.PageData _pageData;

        public MoveObjectCommand(PlacedObjectView view, Vector2 oldPosition, Vector2 newPosition, AdeebTask.Models.PageData pageData)
        {
            _view = view;
            _oldPosition = oldPosition;
            _newPosition = newPosition;
            _pageData = pageData;
        }

        public UniTask ExecuteAsync()
        {
            if (_view != null) _view.SetPosition(_newPosition);
            UpdatePageData(_newPosition);
            return UniTask.CompletedTask;
        }

        public UniTask UndoAsync()
        {
            if (_view != null) _view.SetPosition(_oldPosition);
            UpdatePageData(_oldPosition);
            return UniTask.CompletedTask;
        }

        private void UpdatePageData(Vector2 pos)
        {
            if (_pageData != null && _view != null)
            {
                var data = _pageData.objects.Find(o => o.objectId == _view.ObjectId);
                if (data != null)
                {
                    data.posX = pos.x;
                    data.posY = pos.y;
                }
            }
        }
    }
}
