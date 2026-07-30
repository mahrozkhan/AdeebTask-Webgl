using UnityEngine;
using AdeebTask.Commands;
using AdeebTask.Services.Assets;
using AdeebTask.Views;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace AdeebTask.Controllers.Commands
{
    public class PlaceObjectCommand : ICommand
    {
        private readonly string _assetKey;
        private readonly string _objectId;
        private readonly Vector2 _position;
        private readonly int _sortingOrder;
        private readonly PlacedObjectPool _pool;
        private readonly IAssetService _assetService;
        private readonly Dictionary<string, PlacedObjectView> _activeViews;
        private readonly AdeebTask.Models.PageData _pageData;
        
        private PlacedObjectView _spawnedView;

        public PlaceObjectCommand(
            string assetKey, 
            string objectId, 
            Vector2 position, 
            int sortingOrder, 
            PlacedObjectPool pool, 
            IAssetService assetService,
            Dictionary<string, PlacedObjectView> activeViews,
            AdeebTask.Models.PageData pageData)
        {
            _assetKey = assetKey;
            _objectId = objectId;
            _position = position;
            _sortingOrder = sortingOrder;
            _pool = pool;
            _assetService = assetService;
            _activeViews = activeViews;
            _pageData = pageData;
        }

        public async UniTask ExecuteAsync()
        {
            var handle = await _assetService.AcquireAsync<Sprite>(_assetKey);
            if (handle == null || handle.Asset == null) return;

            _spawnedView = _pool.Acquire();
            _spawnedView.AssetKey = _assetKey; // Save key for deletion undo
            _spawnedView.Initialize(_objectId, handle.Asset, _position, Vector2.one, 0f, _sortingOrder);
            
            _activeViews[_objectId] = _spawnedView;

            if (_pageData != null)
            {
                if (_pageData.objects == null) _pageData.objects = new List<AdeebTask.Models.PlacedObjectData>();
                _pageData.objects.Add(new AdeebTask.Models.PlacedObjectData
                {
                    objectId = _objectId,
                    assetKey = _assetKey,
                    posX = _position.x,
                    posY = _position.y,
                    scaleX = 1f,
                    scaleY = 1f,
                    rotation = 0f,
                    sortingOrder = _sortingOrder
                });
            }
        }

        public async UniTask UndoAsync()
        {
            if (_spawnedView != null)
            {
                _activeViews.Remove(_objectId);
                _pool.Release(_spawnedView);
                _assetService.Release(_assetKey);
                _spawnedView = null;

                if (_pageData != null && _pageData.objects != null)
                {
                    _pageData.objects.RemoveAll(o => o.objectId == _objectId);
                }
            }
            await UniTask.CompletedTask;
        }
    }
}
