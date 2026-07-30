using UnityEngine;

namespace AdeebTask.Views
{
    public enum HandleType
    {
        Scale,
        Rotate
    }

    /// <summary>
    /// Attach this to the corner/top colliders on the SelectionFrame prefab.
    /// It allows the ObjectPlacementController's raycast to identify them as handles instead of the main object.
    /// </summary>
    public class SelectionHandle : MonoBehaviour
    {
        public HandleType HandleType;
    }
}
