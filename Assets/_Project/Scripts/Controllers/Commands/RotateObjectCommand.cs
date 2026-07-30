using UnityEngine;
using AdeebTask.Commands;
using AdeebTask.Views;
using Cysharp.Threading.Tasks;

namespace AdeebTask.Controllers.Commands
{
    public class RotateObjectCommand : ICommand
    {
        private readonly PlacedObjectView _view;
        private readonly float _oldRotation;
        private readonly float _newRotation;
        private readonly AdeebTask.Models.PageData _pageData;

        public RotateObjectCommand(PlacedObjectView view, float oldRotation, float newRotation, AdeebTask.Models.PageData pageData)
        {
            _view = view;
            _oldRotation = oldRotation;
            _newRotation = newRotation;
            _pageData = pageData;
        }

        public UniTask ExecuteAsync()
        {
            if (_view != null) _view.SetRotation(_newRotation);
            UpdatePageData(_newRotation);
            return UniTask.CompletedTask;
        }

        public UniTask UndoAsync()
        {
            if (_view != null) _view.SetRotation(_oldRotation);
            UpdatePageData(_oldRotation);
            return UniTask.CompletedTask;
        }

        private void UpdatePageData(float rotationValue)
        {
            if (_pageData != null && _view != null)
            {
                var data = _pageData.objects.Find(o => o.objectId == _view.ObjectId);
                if (data != null)
                {
                    data.rotation = rotationValue;
                }
            }
        }
    }
}
