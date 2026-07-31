using UnityEngine;
using AdeebTask.Core.Events;
using AdeebTask.Core;
using TMPro;

namespace AdeebTask.UI.Screens
{
    public class ConfirmationPopupScreen : AppScreen
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private GameObject _buttonsContainer; 
        [SerializeField] private GameObject _confirmButton;    
        [SerializeField] private GameObject _cancelButton;     
        
        private IEventBus _eventBus;
        private string _currentPopupId;

        public override void Initialize()
        {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _eventBus.Subscribe<ShowConfirmationPopupEvent>(HandleShowPopup);
        }

        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<ShowConfirmationPopupEvent>(HandleShowPopup);
            }
        }

        private void HandleShowPopup(ShowConfirmationPopupEvent evt)
        {
            _currentPopupId = evt.PopupId;
            
            if (_titleText != null) _titleText.text = evt.Title;
            if (_messageText != null) _messageText.text = evt.Message;
            
            CancelInvoke(nameof(AutoClose)); // Stop any existing timer

            switch (evt.Type)
            {
                case PopupType.StandardConfirm:
                    if (_buttonsContainer != null) _buttonsContainer.SetActive(true);
                    if (_confirmButton != null) _confirmButton.SetActive(true);
                    if (_cancelButton != null) _cancelButton.SetActive(true);
                    break;

                case PopupType.Error:
                    if (_buttonsContainer != null) _buttonsContainer.SetActive(true);
                    if (_confirmButton != null) _confirmButton.SetActive(true);
                    if (_cancelButton != null) _cancelButton.SetActive(false); // Hide cancel button
                    break;

                case PopupType.ConnectionError:
                    if (_buttonsContainer != null) _buttonsContainer.SetActive(false);
                    if (_confirmButton != null) _confirmButton.SetActive(false);
                    if (_cancelButton != null) _cancelButton.SetActive(false);
                    Invoke(nameof(AutoClose), 3.0f); // Auto-close after 3 seconds
                    break;
            }

            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        private void AutoClose()
        {
            if (_eventBus != null) _eventBus.Publish(new ConfirmationPopupResponseEvent(_currentPopupId, false));
            gameObject.SetActive(false);
        }

        public void HandleConfirmClicked()
        {
            if (_eventBus != null) _eventBus.Publish(new ConfirmationPopupResponseEvent(_currentPopupId, true));
            gameObject.SetActive(false);
        }

        public void HandleCancelClicked()
        {
            if (_eventBus != null) _eventBus.Publish(new ConfirmationPopupResponseEvent(_currentPopupId, false));
            gameObject.SetActive(false);
        }
    }
}
