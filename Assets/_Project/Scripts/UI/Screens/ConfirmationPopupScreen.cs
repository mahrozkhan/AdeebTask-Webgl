using System;
using UnityEngine;
using AdeebTask.UI;
using AdeebTask.Core.Events;

namespace AdeebTask.UI.Screens
{
    public class ConfirmationPopupScreen : AppScreen
    {
        [SerializeField] private TMPro.TextMeshProUGUI _titleText;
        [SerializeField] private TMPro.TextMeshProUGUI _messageText;
        
        private IEventBus _eventBus;
        private string _currentPopupId;

        public override void Initialize()
        {
            _eventBus = Core.ServiceLocator.Get<IEventBus>();
            _eventBus.Subscribe<Core.Events.ShowConfirmationPopupEvent>(HandleShowPopup);
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<Core.Events.ShowConfirmationPopupEvent>(HandleShowPopup);
            }
        }

        private void HandleShowPopup(Core.Events.ShowConfirmationPopupEvent evt)
        {
            _currentPopupId = evt.PopupId;
            
            if (_titleText != null) _titleText.text = evt.Title;
            if (_messageText != null) _messageText.text = evt.Message;
            
            // Show additively to prevent hiding the active screen underneath
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        public void HandleConfirmClicked()
        {
            if (_eventBus != null) _eventBus.Publish(new Core.Events.ConfirmationPopupResponseEvent(_currentPopupId, true));
            gameObject.SetActive(false);
        }

        public void HandleCancelClicked()
        {
            if (_eventBus != null) _eventBus.Publish(new Core.Events.ConfirmationPopupResponseEvent(_currentPopupId, false));
            gameObject.SetActive(false);
        }
    }
}
