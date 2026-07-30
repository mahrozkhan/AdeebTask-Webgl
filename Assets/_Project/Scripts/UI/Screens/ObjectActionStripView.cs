using System;
using UnityEngine;
using AdeebTask.Core;
using AdeebTask.Core.Events;

namespace AdeebTask.UI.Screens
{
    public class ObjectActionStripView : MonoBehaviour
    {
        private IEventBus _eventBus;

        private void Start()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _eventBus.Subscribe<ObjectSelectedEvent>(OnObjectSelected);
            _eventBus.Subscribe<ObjectDeselectedEvent>(OnObjectDeselected);
            
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<ObjectSelectedEvent>(OnObjectSelected);
                _eventBus.Unsubscribe<ObjectDeselectedEvent>(OnObjectDeselected);
            }
        }

        private void OnObjectSelected(ObjectSelectedEvent evt) => gameObject.SetActive(true);
        private void OnObjectDeselected(ObjectDeselectedEvent evt) => gameObject.SetActive(false);

        // Called by UI Button UnityEvents
        public void HandleDeleteClicked() => _eventBus.Publish(new ActionStripDeleteRequestedEvent());
        
        // Called by UI Button UnityEvents
        public void HandleMirrorClicked() => _eventBus.Publish(new ActionStripMirrorRequestedEvent());

        // Called by UI Button UnityEvents
        public void HandleDuplicateClicked() => _eventBus.Publish(new ActionStripDuplicateRequestedEvent());

        // Called by UI Button UnityEvents
        public void HandleConfirmClicked() => _eventBus.Publish(new ActionStripConfirmRequestedEvent());
    }
}
