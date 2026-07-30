using UnityEngine;
using AdeebTask.Commands;
using AdeebTask.Services.Assets;
using AdeebTask.Views;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace AdeebTask.Controllers.Commands
{
    public class DeleteObjectCommand : ICommand
    {
        private readonly string _objectId;
        private readonly PlacedObjectPool _pool;
        private readonly IAssetService _assetService;
        private readonly Dictionary<string, PlacedObjectView> _activeViews;
        private readonly AdeebTask.Models.PageData _pageData;
        
        // Stored for undo
        private string _assetKey;
        private Vector2 _position;
        private Vector2 _scale;
        private float _rotation;
        private int _sortingOrder;
        private PlacedObjectView _viewToRestore;

        public DeleteObjectCommand(
            string objectId, 
            PlacedObjectPool pool, 
            IAssetService assetService,
            Dictionary<string, PlacedObjectView> activeViews,
            AdeebTask.Models.PageData pageData)
        {
            _objectId = objectId;
            _pool = pool;
            _assetService = assetService;
            _activeViews = activeViews;
            _pageData = pageData;
        }

        public UniTask ExecuteAsync()
        {
            if (_activeViews.TryGetValue(_objectId, out var view))
            {
                _assetKey = view.AssetKey;
                _position = view.transform.position;
                _scale = view.transform.localScale;
                _rotation = view.transform.rotation.eulerAngles.z;
                _sortingOrder = view.SortingOrder;

                _activeViews.Remove(_objectId);
                _pool.Release(view);
                _assetService.Release(_assetKey);

                if (_pageData != null && _pageData.objects != null)
                {
                    _pageData.objects.RemoveAll(o => o.objectId == _objectId);
                }
            }
            return UniTask.CompletedTask;
        }

        public async UniTask UndoAsync()
        {
            if (string.IsNullOrEmpty(_assetKey)) return;

            var handle = await _assetService.AcquireAsync<Sprite>(_assetKey);
            if (handle == null || handle.Asset == null) return;

            _viewToRestore = _pool.Acquire();
            _viewToRestore.AssetKey = _assetKey; 
            _viewToRestore.Initialize(_objectId, handle.Asset, _position, _scale, _rotation, _sortingOrder);
            
            _activeViews[_objectId] = _viewToRestore;

            if (_pageData != null)
            {
                if (_pageData.objects == null) _pageData.objects = new List<AdeebTask.Models.PlacedObjectData>();
                _pageData.objects.Add(new AdeebTask.Models.PlacedObjectData
                {
                    objectId = _objectId,
                    assetKey = _assetKey,
                    posX = _position.x,
                    posY = _position.y,
                    scaleX = _scale.x,
                    scaleY = _scale.y,
                    rotation = _rotation,
                    sortingOrder = _sortingOrder
                });
            }
        }
    }
}
