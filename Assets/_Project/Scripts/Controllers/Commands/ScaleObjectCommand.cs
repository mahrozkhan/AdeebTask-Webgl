using UnityEngine;
using AdeebTask.Commands;
using AdeebTask.Views;
using Cysharp.Threading.Tasks;

namespace AdeebTask.Controllers.Commands
{
    public class ScaleObjectCommand : ICommand
    {
        private readonly PlacedObjectView _view;
        private readonly Vector2 _oldScale;
        private readonly Vector2 _newScale;
        private readonly AdeebTask.Models.PageData _pageData;

        public ScaleObjectCommand(PlacedObjectView view, Vector2 oldScale, Vector2 newScale, AdeebTask.Models.PageData pageData)
        {
            _view = view;
            _oldScale = oldScale;
            _newScale = newScale;
            _pageData = pageData;
        }

        public UniTask ExecuteAsync()
        {
            if (_view != null) _view.SetScale(_newScale);
            UpdatePageData(_newScale);
            return UniTask.CompletedTask;
        }

        public UniTask UndoAsync()
        {
            if (_view != null) _view.SetScale(_oldScale);
            UpdatePageData(_oldScale);
            return UniTask.CompletedTask;
        }

        private void UpdatePageData(Vector2 scale)
        {
            if (_pageData != null && _view != null)
            {
                var data = _pageData.objects.Find(o => o.objectId == _view.ObjectId);
                if (data != null)
                {
                    data.scaleX = scale.x;
                    data.scaleY = scale.y;
                }
            }
        }
    }
}
