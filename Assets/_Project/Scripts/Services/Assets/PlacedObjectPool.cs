using System.Collections.Generic;
using UnityEngine;
using AdeebTask.Views;

namespace AdeebTask.Services.Assets
{
    public class PlacedObjectPool : MonoBehaviour
    {
        [SerializeField] private PlacedObjectView _prefab;
        [SerializeField] private int _initialPoolSize = 20;

        private readonly Queue<PlacedObjectView> _pool = new Queue<PlacedObjectView>();
        private readonly List<PlacedObjectView> _activeObjects = new List<PlacedObjectView>();

        private void Awake()
        {
            if (_prefab == null)
            {
                Debug.LogWarning("[PlacedObjectPool] Prefab is not assigned.");
                return;
            }

            for (int i = 0; i < _initialPoolSize; i++)
            {
                CreateNewObject();
            }
        }

        private PlacedObjectView CreateNewObject()
        {
            var obj = Instantiate(_prefab, transform);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
            return obj;
        }

        public PlacedObjectView Acquire()
        {
            if (_pool.Count == 0)
            {
                CreateNewObject();
            }

            var obj = _pool.Dequeue();
            _activeObjects.Add(obj);
            return obj;
        }

        public void Release(PlacedObjectView obj)
        {
            if (_activeObjects.Remove(obj))
            {
                obj.Clear();
                _pool.Enqueue(obj);
            }
        }

        public void ReleaseAll()
        {
            foreach (var obj in _activeObjects)
            {
                obj.Clear();
                _pool.Enqueue(obj);
            }
            _activeObjects.Clear();
        }
    }
}
