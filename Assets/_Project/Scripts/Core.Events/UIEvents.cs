using AdeebTask.Models;

namespace AdeebTask.Core.Events
{
    public readonly struct SpawnObjectRequestedEvent
    {
        public readonly string AddressableKey;
        public SpawnObjectRequestedEvent(string addressableKey) => AddressableKey = addressableKey;
    }

    public readonly struct SaveProjectRequestedEvent 
    {
        public readonly bool NavigateToMenuAfterSave;

        public SaveProjectRequestedEvent(bool navigateToMenuAfterSave = true)
        {
            NavigateToMenuAfterSave = navigateToMenuAfterSave;
        }
    }

    public readonly struct SetBackgroundRequestedEvent
    {
        public readonly string AddressableKey;
        public SetBackgroundRequestedEvent(string addressableKey) => AddressableKey = addressableKey;
    }

    public readonly struct BackgroundUpdatedEvent
    {
        public readonly string AddressableKey;
        public BackgroundUpdatedEvent(string addressableKey) => AddressableKey = addressableKey;
    }

    public readonly struct ProjectSetupConfirmedEvent
    {
        public readonly string BackgroundKey;
        
        public ProjectSetupConfirmedEvent(string backgroundKey)
        {
            BackgroundKey = backgroundKey;
        }
    }

    public readonly struct ProjectSetupCancelledEvent { }

    public readonly struct CreateNewProjectRequestedEvent { }

    public readonly struct OpenProjectRequestedEvent
    {
        public readonly string ProjectId;
        public OpenProjectRequestedEvent(string projectId) => ProjectId = projectId;
    }

    public readonly struct NextPageRequestedEvent { }
    public readonly struct PrevPageRequestedEvent { }
    public readonly struct AddPageRequestedEvent { }
    public readonly struct DeletePageRequestedEvent { }
    public readonly struct EditorQuitRequestedEvent { }

    public readonly struct ShowConfirmationPopupEvent
    {
        public readonly string PopupId;
        public readonly string Title;
        public readonly string Message;

        public ShowConfirmationPopupEvent(string popupId, string title, string message)
        {
            PopupId = popupId;
            Title = title;
            Message = message;
        }
    }

    public readonly struct ConfirmationPopupResponseEvent
    {
        public readonly string PopupId;
        public readonly bool IsConfirmed;

        public ConfirmationPopupResponseEvent(string popupId, bool isConfirmed)
        {
            PopupId = popupId;
            IsConfirmed = isConfirmed;
        }
    }

    public readonly struct EditorModeChangedEvent
    {
        public readonly bool IsViewOnly;

        public EditorModeChangedEvent(bool isViewOnly)
        {
            IsViewOnly = isViewOnly;
        }
    }

    public readonly struct PageLoadedEvent 
    {
        public readonly AdeebTask.Models.PageData PageData;
        public PageLoadedEvent(AdeebTask.Models.PageData pageData) => PageData = pageData;
    }

    public readonly struct PageNavigationStateChangedEvent
    {
        public readonly int CurrentPageIndex;
        public readonly int TotalPages;
        public PageNavigationStateChangedEvent(int currentPageIndex, int totalPages)
        {
            CurrentPageIndex = currentPageIndex;
            TotalPages = totalPages;
        }
    }

    // Action Strip Events
    public readonly struct ActionStripDeleteRequestedEvent { }
    public readonly struct ActionStripMirrorRequestedEvent { }
    public readonly struct ActionStripDuplicateRequestedEvent { }
    public readonly struct ActionStripConfirmRequestedEvent { }

    // Object Selection Events
    public readonly struct RequestObjectSelectionEvent 
    { 
        public readonly AdeebTask.Views.PlacedObjectView View;
        public RequestObjectSelectionEvent(AdeebTask.Views.PlacedObjectView view) => View = view;
    }
    public readonly struct RequestObjectDeselectionEvent { }

    public readonly struct SelectionChangedEvent 
    { 
        public readonly AdeebTask.Views.PlacedObjectView SelectedView;
        public SelectionChangedEvent(AdeebTask.Views.PlacedObjectView selectedView) => SelectedView = selectedView;
    }
    public readonly struct ObjectSelectedEvent { } // UI trigger
    public readonly struct ObjectDeselectedEvent { } // UI trigger
}
