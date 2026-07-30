namespace AdeebTask.Core.Events
{
    public readonly struct NavigateToEditorEvent 
    { 
        public readonly string ProjectId;
        public readonly bool IsNewProject;
        public NavigateToEditorEvent(string projectId, bool isNewProject = false) 
        {
            ProjectId = projectId;
            IsNewProject = isNewProject;
        }
    }
    
    public readonly struct NavigateToPlaybackEvent 
    { 
        public readonly string ProjectId; 
        public NavigateToPlaybackEvent(string projectId) => ProjectId = projectId;
    }
    
    public readonly struct NavigateToMenuEvent { }
    
    public readonly struct NavigateToProjectSetupEvent { }
}
