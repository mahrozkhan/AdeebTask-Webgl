using UnityEngine;

namespace AdeebTask.UI
{
    public abstract class AppScreen : MonoBehaviour
    {
        public virtual void Initialize() { }

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }

    public abstract class AppScreen<TData> : AppScreen
    {
        public abstract void Show(TData data);
        
        // Hide the parameterless Show to force data injection
        public override void Show()
        {
            Debug.LogError($"[AppScreen] Screen {GetType().Name} requires data of type {typeof(TData).Name} to show.");
        }
    }
}
