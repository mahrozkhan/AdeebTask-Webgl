using System;
using UnityEngine;
using UnityEngine.UI;
using AdeebTask.Core;
using AdeebTask.Core.Events;

namespace AdeebTask.UI.Screens
{
    public class PageNavigationScreen : MonoBehaviour
    {
        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _addButton;
        [SerializeField] private Button _deleteButton;

        private IEventBus _eventBus;
        private bool _isViewOnly;

        private void Awake()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _eventBus.Subscribe<PageNavigationStateChangedEvent>(HandleStateChanged);
            _eventBus.Subscribe<EditorModeChangedEvent>(HandleModeChanged);
        }

        private void Start()
        {
            if (_prevButton != null) 
            {
                _prevButton.onClick.AddListener(HandlePrevClicked);
                _prevButton.gameObject.SetActive(false);
            }
            if (_nextButton != null) 
            {
                _nextButton.onClick.AddListener(HandleNextClicked);
                _nextButton.gameObject.SetActive(false);
            }
            if (_addButton != null) _addButton.onClick.AddListener(HandleAddPageClicked);
            if (_deleteButton != null) _deleteButton.onClick.AddListener(HandleDeletePageClicked);
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<PageNavigationStateChangedEvent>(HandleStateChanged);
                _eventBus.Unsubscribe<EditorModeChangedEvent>(HandleModeChanged);
            }
        }

        private void HandleStateChanged(PageNavigationStateChangedEvent evt)
        {
            if (_prevButton != null) _prevButton.gameObject.SetActive(evt.CurrentPageIndex > 0);
            if (_nextButton != null) _nextButton.gameObject.SetActive(evt.CurrentPageIndex < evt.TotalPages - 1);
            if (_deleteButton != null) _deleteButton.interactable = evt.TotalPages > 1;

            if (_addButton != null) 
            {
                _addButton.gameObject.SetActive(!_isViewOnly && evt.CurrentPageIndex == evt.TotalPages - 1);
            }
        }

        private void HandleModeChanged(EditorModeChangedEvent evt)
        {
            _isViewOnly = evt.IsViewOnly;
            
            if (_deleteButton != null) _deleteButton.gameObject.SetActive(!_isViewOnly);
            
            // If switching to view mode, ensure add button is hidden immediately. 
            // When switching to edit mode, it will remain hidden until a StateChanged event evaluates the page index.
            if (_addButton != null && _isViewOnly) _addButton.gameObject.SetActive(false);
        }

        private void HandleNextClicked() => _eventBus.Publish(new NextPageRequestedEvent());
        private void HandlePrevClicked() => _eventBus.Publish(new PrevPageRequestedEvent());
        private void HandleAddPageClicked() => _eventBus.Publish(new AddPageRequestedEvent());
        private void HandleDeletePageClicked() => _eventBus.Publish(new DeletePageRequestedEvent());
    }
}
