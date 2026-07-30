using UnityEngine;

namespace AdeebTask.Views
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlacedObjectView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        
        public string ObjectId { get; private set; }
        public string AssetKey { get; set; }
        public int SortingOrder => _spriteRenderer != null ? _spriteRenderer.sortingOrder : 0;

        private BoxCollider2D _collider;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
        }

        public void Initialize(string objectId, Sprite sprite, Vector2 position, Vector2 scale, float rotation, int sortingOrder)
        {
            ObjectId = objectId;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sprite = sprite;
                _spriteRenderer.sortingOrder = sortingOrder;
            }
            if (_collider != null && sprite != null)
            {
                _collider.size = sprite.bounds.size;
            }
            
            transform.position = position;
            transform.localScale = scale;
            transform.rotation = Quaternion.Euler(0, 0, rotation);
            gameObject.SetActive(true);
        }

        public void SetPosition(Vector2 position) => transform.position = position;
        public void SetScale(Vector2 scale) => transform.localScale = scale;
        public void SetRotation(float angle) => transform.rotation = Quaternion.Euler(0, 0, angle);
        public void SetSortingOrder(int order)
        {
            if (_spriteRenderer != null) _spriteRenderer.sortingOrder = order;
        }

        public void Clear()
        {
            ObjectId = null;
            AssetKey = null;
            if (_spriteRenderer != null) _spriteRenderer.sprite = null;
            gameObject.SetActive(false);
        }
    }
}
