using System;
using System.Collections.Generic;
using UnityEngine;
using AdeebTask.Commands;
using AdeebTask.Services.Assets;
using AdeebTask.Views;
using AdeebTask.Controllers.Commands;
using AdeebTask.Core;
using AdeebTask.Core.Events;
using AdeebTask.Models;
using AdeebTask.UI.Screens;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;

namespace AdeebTask.Controllers
{
    public class ObjectPlacementController : MonoBehaviour
    {
        [SerializeField] private PlacedObjectPool _pool;

        private CommandExecutor _commandExecutor;
        private IAssetService _assetService;
        private IEventBus _eventBus;
        
        private readonly Dictionary<string, PlacedObjectView> _activeViews = new Dictionary<string, PlacedObjectView>();
        
        // State for dragging
        private PlacedObjectView _currentlyDragging;
        private Vector2 _dragStartPosition;
        private Camera _mainCamera;
        private PlacedObjectView _currentSelection;
        private int _currentSortingOrder = Constants.ObjectStartingSortingOrder;

        private PageData _currentPageData;
        private bool _isViewOnly = false;

        // Interaction States
        private enum DragState { None, Moving, Scaling, Rotating }
        private DragState _dragState = DragState.None;
        
        // Transform memory for Undo/Redo tracking
        private Vector2 _dragStartScale;
        private float _dragStartRotation;
        private Vector2 _dragStartMouseWorldPos;
        private Vector2 _objectCenterOnDragStart;

        private void Start()
        {
            _mainCamera = Camera.main;
            _commandExecutor = new CommandExecutor();
            _assetService = ServiceLocator.Get<IAssetService>();
            _eventBus = ServiceLocator.Get<IEventBus>();

            _eventBus.Subscribe<SpawnObjectRequestedEvent>(HandleSpawnObject);
            _eventBus.Subscribe<PageLoadedEvent>(HandlePageLoaded);
            
            _eventBus.Subscribe<ActionStripDeleteRequestedEvent>(HandleDeleteClicked);
            _eventBus.Subscribe<ActionStripMirrorRequestedEvent>(HandleMirrorClicked);
            _eventBus.Subscribe<ActionStripDuplicateRequestedEvent>(HandleDuplicateClicked);
            _eventBus.Subscribe<ActionStripConfirmRequestedEvent>(HandleConfirmClicked);
            _eventBus.Subscribe<SelectionChangedEvent>(HandleSelectionChanged);
            _eventBus.Subscribe<NavigateToMenuEvent>(HandleNavigateToMenu);
            _eventBus.Subscribe<EditorModeChangedEvent>(HandleModeChanged);
        }

        private void OnDestroy()
        {
            if (_eventBus != null) 
            {
                _eventBus.Unsubscribe<SpawnObjectRequestedEvent>(HandleSpawnObject);
                _eventBus.Unsubscribe<PageLoadedEvent>(HandlePageLoaded);
                _eventBus.Unsubscribe<ActionStripDeleteRequestedEvent>(HandleDeleteClicked);
                _eventBus.Unsubscribe<ActionStripMirrorRequestedEvent>(HandleMirrorClicked);
                _eventBus.Unsubscribe<ActionStripDuplicateRequestedEvent>(HandleDuplicateClicked);
                _eventBus.Unsubscribe<ActionStripConfirmRequestedEvent>(HandleConfirmClicked);
                _eventBus.Unsubscribe<SelectionChangedEvent>(HandleSelectionChanged);
                _eventBus.Unsubscribe<NavigateToMenuEvent>(HandleNavigateToMenu);
                _eventBus.Unsubscribe<EditorModeChangedEvent>(HandleModeChanged);
            }
        }

        private void HandleSelectionChanged(SelectionChangedEvent evt)
        {
            _currentSelection = evt.SelectedView;
        }

        private void HandleNavigateToMenu(NavigateToMenuEvent evt)
        {
            ClearAll();
            _currentPageData = null;
        }

        private void HandleModeChanged(EditorModeChangedEvent evt)
        {
            _isViewOnly = evt.IsViewOnly;
            if (_isViewOnly)
            {
                _eventBus.Publish(new RequestObjectDeselectionEvent());
                _eventBus.Publish(new ObjectDeselectedEvent());
            }
        }

        private async void HandlePageLoaded(PageLoadedEvent evt)
        {
            _currentPageData = evt.PageData;
            ClearAll();

            if (_currentPageData != null && _currentPageData.objects != null)
            {
                foreach (var objData in _currentPageData.objects)
                {
                    await SpawnObjectFromSaveAsync(objData);
                }
            }
        }

        private async void HandleSpawnObject(SpawnObjectRequestedEvent evt)
        {
            if (_currentPageData == null) return;

            string objectId = Guid.NewGuid().ToString();
            Vector2 screenCenter = _mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, 10f));
            
            var command = new PlaceObjectCommand(evt.AddressableKey, objectId, screenCenter, _currentSortingOrder++, _pool, _assetService, _activeViews, _currentPageData);
            await _commandExecutor.ExecuteAsync(command);

            if (_activeViews.TryGetValue(objectId, out var view))
            {
                _eventBus.Publish(new RequestObjectSelectionEvent(view));
                _eventBus.Publish(new ObjectSelectedEvent());
            }
        }

        private void HandleDeleteClicked(ActionStripDeleteRequestedEvent evt)
        {
            if (_currentSelection != null && _currentPageData != null)
            {
                var command = new DeleteObjectCommand(_currentSelection.ObjectId, _pool, _assetService, _activeViews, _currentPageData);
                _commandExecutor.ExecuteAsync(command).Forget();
                _eventBus.Publish(new RequestObjectDeselectionEvent());
                _eventBus.Publish(new ObjectDeselectedEvent());
            }
        }

        private void HandleMirrorClicked(ActionStripMirrorRequestedEvent evt)
        {
            if (_currentSelection != null)
            {
                var command = new MirrorObjectCommand(_currentSelection);
                _commandExecutor.ExecuteAsync(command).Forget();
            }
        }

        private void HandleDuplicateClicked(ActionStripDuplicateRequestedEvent evt)
        {
            if (_currentSelection != null && _currentPageData != null)
            {
                string assetKey = _currentSelection.AssetKey;
                Vector2 currentPos = _currentSelection.transform.position;
                Vector2 offsetPos = currentPos + new Vector2(0.5f, -0.5f); // Offset the duplicate slightly
                
                string newId = Guid.NewGuid().ToString();
                
                var command = new PlaceObjectCommand(assetKey, newId, offsetPos, _currentSortingOrder++, _pool, _assetService, _activeViews, _currentPageData);
                _commandExecutor.ExecuteAsync(command).Forget();
                
                // Deselect the old one, the new one will be selectable by clicking
                _eventBus.Publish(new RequestObjectDeselectionEvent());
                _eventBus.Publish(new ObjectDeselectedEvent());
            }
        }

        private void HandleConfirmClicked(ActionStripConfirmRequestedEvent evt)
        {
            _eventBus.Publish(new RequestObjectDeselectionEvent());
            _eventBus.Publish(new ObjectDeselectedEvent());
        }

        private void Update()
        {
            HandleMouseInput();
        }

        private void HandleMouseInput()
        {
            if (_isViewOnly || _mainCamera == null || Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // Allocation-free standard check. Requires UI background elements to have RaycastTarget = false.
                if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

                Vector2 mousePos = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
                
                AdeebTask.Views.SelectionHandle hitHandle = null;
                PlacedObjectView hitObject = null;

                foreach (var hit in hits)
                {
                    var handle = hit.collider.GetComponent<AdeebTask.Views.SelectionHandle>();
                    if (handle != null) hitHandle = handle;
                    
                    var obj = hit.collider.GetComponent<PlacedObjectView>();
                    if (obj != null && hitObject == null) hitObject = obj; // Grab the first (highest Z/sorting typically)
                }

                if (hitHandle != null && _currentSelection != null)
                {
                    // User clicked a scale or rotate handle on the active selection
                    _currentlyDragging = _currentSelection;
                    _dragStartMouseWorldPos = mousePos;
                    _objectCenterOnDragStart = _currentlyDragging.transform.position;
                    
                    if (hitHandle.HandleType == AdeebTask.Views.HandleType.Scale)
                    {
                        _dragState = DragState.Scaling;
                        _dragStartScale = _currentlyDragging.transform.localScale;
                    }
                    else if (hitHandle.HandleType == AdeebTask.Views.HandleType.Rotate)
                    {
                        _dragState = DragState.Rotating;
                        _dragStartRotation = _currentlyDragging.transform.eulerAngles.z;
                    }
                }
                else if (hitObject != null)
                {
                    // User clicked an object body
                    _currentlyDragging = hitObject;
                    _dragState = DragState.Moving;
                    _dragStartPosition = _currentlyDragging.transform.position;
                    _dragStartMouseWorldPos = mousePos;
                    
                    _eventBus.Publish(new RequestObjectSelectionEvent(hitObject));
                    _eventBus.Publish(new ObjectSelectedEvent());
                }
                else
                {
                    // Clicked empty space
                    _eventBus.Publish(new RequestObjectDeselectionEvent());
                    _eventBus.Publish(new ObjectDeselectedEvent());
                }
            }
            else if (Mouse.current.leftButton.isPressed && _currentlyDragging != null)
            {
                Vector2 mousePos = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                
                if (_dragState == DragState.Moving)
                {
                    Vector2 delta = mousePos - _dragStartMouseWorldPos;
                    _currentlyDragging.SetPosition(_dragStartPosition + delta);
                }
                else if (_dragState == DragState.Scaling)
                {
                    // Uniform scaling based on distance from center
                    float startDist = Vector2.Distance(_objectCenterOnDragStart, _dragStartMouseWorldPos);
                    float currentDist = Vector2.Distance(_objectCenterOnDragStart, mousePos);
                    
                    if (startDist > 0.01f) // Prevent divide by zero
                    {
                        float scaleFactor = currentDist / startDist;
                        _currentlyDragging.SetScale(_dragStartScale * scaleFactor);
                    }
                }
                else if (_dragState == DragState.Rotating)
                {
                    Vector2 startDir = _dragStartMouseWorldPos - _objectCenterOnDragStart;
                    Vector2 currentDir = mousePos - _objectCenterOnDragStart;
                    
                    float angle = Vector2.SignedAngle(startDir, currentDir);
                    _currentlyDragging.SetRotation(_dragStartRotation + angle);
                }
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame && _currentlyDragging != null)
            {
                if (_dragState == DragState.Moving)
                {
                    Vector2 dragEndPosition = _currentlyDragging.transform.position;
                    if (Vector2.Distance(_dragStartPosition, dragEndPosition) > 0.01f)
                    {
                        var command = new MoveObjectCommand(_currentlyDragging, _dragStartPosition, dragEndPosition, _currentPageData);
                        _commandExecutor.ExecuteAsync(command).Forget();
                    }
                }
                else if (_dragState == DragState.Scaling)
                {
                    Vector2 dragEndScale = _currentlyDragging.transform.localScale;
                    if (Vector2.Distance(_dragStartScale, dragEndScale) > 0.01f)
                    {
                        var command = new AdeebTask.Controllers.Commands.ScaleObjectCommand(_currentlyDragging, _dragStartScale, dragEndScale, _currentPageData);
                        _commandExecutor.ExecuteAsync(command).Forget();
                    }
                }
                else if (_dragState == DragState.Rotating)
                {
                    float dragEndRotation = _currentlyDragging.transform.eulerAngles.z;
                    if (Mathf.Abs(Mathf.DeltaAngle(_dragStartRotation, dragEndRotation)) > 0.1f)
                    {
                        var command = new AdeebTask.Controllers.Commands.RotateObjectCommand(_currentlyDragging, _dragStartRotation, dragEndRotation, _currentPageData);
                        _commandExecutor.ExecuteAsync(command).Forget();
                    }
                }

                _currentlyDragging = null;
                _dragState = DragState.None;
            }
        }
        
        public void ClearAll()
        {
            _activeViews.Clear();
            _pool.ReleaseAll();
            _currentSortingOrder = Constants.ObjectStartingSortingOrder;
            _eventBus.Publish(new RequestObjectDeselectionEvent());
            _eventBus.Publish(new ObjectDeselectedEvent());
        }

        public async UniTask SpawnObjectFromSaveAsync(PlacedObjectData data)
        {
            var handle = await _assetService.AcquireAsync<Sprite>(data.assetKey);
            if (handle == null || handle.Asset == null) return;

            var spawnedView = _pool.Acquire();
            spawnedView.AssetKey = data.assetKey;
            spawnedView.Initialize(
                data.objectId, 
                handle.Asset, 
                new Vector2(data.posX, data.posY), 
                new Vector2(data.scaleX, data.scaleY), 
                data.rotation, 
                data.sortingOrder
            );
            
            _activeViews[data.objectId] = spawnedView;
            
            if (data.sortingOrder >= _currentSortingOrder)
            {
                _currentSortingOrder = data.sortingOrder + 1;
            }
        }
    }
}
