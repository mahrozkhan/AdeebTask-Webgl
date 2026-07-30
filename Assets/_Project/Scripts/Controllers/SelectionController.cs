using System;
using UnityEngine;
using AdeebTask.Views;
using AdeebTask.Core.Events;

namespace AdeebTask.Controllers
{
    public class SelectionController : MonoBehaviour
    {
        [SerializeField] private GameObject _selectionFrame; 
        [SerializeField] private SpriteRenderer _frameBorder; 
        
        [Header("Handles")]
        [SerializeField] private Transform _topLeftHandle;
        [SerializeField] private Transform _topRightHandle;
        [SerializeField] private Transform _bottomLeftHandle;
        [SerializeField] private Transform _bottomRightHandle;
        [SerializeField] private Transform _rotateHandle;

        private IEventBus _eventBus;
        
        public PlacedObjectView SelectedObject { get; private set; }

        private void Start()
        {
            _eventBus = AdeebTask.Core.ServiceLocator.Get<IEventBus>();
            _eventBus.Subscribe<AdeebTask.Core.Events.RequestObjectSelectionEvent>(HandleSelectionRequest);
            _eventBus.Subscribe<AdeebTask.Core.Events.RequestObjectDeselectionEvent>(HandleDeselectionRequest);

            if (_selectionFrame != null)
            {
                _selectionFrame.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<AdeebTask.Core.Events.RequestObjectSelectionEvent>(HandleSelectionRequest);
                _eventBus.Unsubscribe<AdeebTask.Core.Events.RequestObjectDeselectionEvent>(HandleDeselectionRequest);
            }
        }

        private void HandleSelectionRequest(AdeebTask.Core.Events.RequestObjectSelectionEvent evt) => SelectObject(evt.View);
        private void HandleDeselectionRequest(AdeebTask.Core.Events.RequestObjectDeselectionEvent evt) => Deselect();

        public void SelectObject(PlacedObjectView view)
        {
            if (SelectedObject == view) return;
            
            SelectedObject = view;
            
            if (SelectedObject != null && _selectionFrame != null)
            {
                _selectionFrame.SetActive(true);
                UpdateFrameTransform();
                _eventBus.Publish(new AdeebTask.Core.Events.SelectionChangedEvent(SelectedObject));
            }
            else
            {
                Deselect();
            }
        }

        public void Deselect()
        {
            if (SelectedObject != null)
            {
                SelectedObject = null;
                if (_selectionFrame != null) _selectionFrame.SetActive(false);
                _eventBus.Publish(new AdeebTask.Core.Events.SelectionChangedEvent(null));
            }
        }

        private void Update()
        {
            if (SelectedObject != null && _selectionFrame != null && _selectionFrame.activeSelf)
            {
                UpdateFrameTransform();
            }
        }

        private void UpdateFrameTransform()
        {
            // Position and rotation always match the target
            _selectionFrame.transform.position = SelectedObject.transform.position;
            _selectionFrame.transform.rotation = SelectedObject.transform.rotation;

            // Optional: If you want the frame to stretch to fit the sprite bounds perfectly
            var spriteRenderer = SelectedObject.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null && _frameBorder != null)
            {
                // We use localScale of the target to size the border
                // This assumes the frame prefab is built 1x1 unit in size
                Vector2 size = spriteRenderer.sprite.bounds.size;
                Vector2 targetScale = SelectedObject.transform.localScale;
                
                // Absolute scale to handle mirroring (negative x)
                Vector2 finalSize = new Vector2(size.x * Mathf.Abs(targetScale.x), size.y * Mathf.Abs(targetScale.y));
                _frameBorder.size = finalSize;

                // Reposition Handles to the corners of the border
                Vector2 halfSize = finalSize / 2f;
                if (_topLeftHandle != null) _topLeftHandle.localPosition = new Vector2(-halfSize.x, halfSize.y);
                if (_topRightHandle != null) _topRightHandle.localPosition = new Vector2(halfSize.x, halfSize.y);
                if (_bottomLeftHandle != null) _bottomLeftHandle.localPosition = new Vector2(-halfSize.x, -halfSize.y);
                if (_bottomRightHandle != null) _bottomRightHandle.localPosition = new Vector2(halfSize.x, -halfSize.y);
                if (_rotateHandle != null) _rotateHandle.localPosition = new Vector2(0, halfSize.y + 0.5f); // Slightly above top
            }
        }

#if UNITY_EDITOR
        public void SetSelectionFrameData(GameObject frame, SpriteRenderer border, Transform tl, Transform tr, Transform bl, Transform br, Transform rot)
        {
            _selectionFrame = frame;
            _frameBorder = border;
            _topLeftHandle = tl;
            _topRightHandle = tr;
            _bottomLeftHandle = bl;
            _bottomRightHandle = br;
            _rotateHandle = rot;
        }
#endif
    }
}
