using UnityEngine;

namespace AdeebTask.UI.Virtualization
{
    public interface IVirtualScrollCell<TData>
    {
        RectTransform RectTransform { get; }
        void Bind(TData data);
        void SetActive(bool isActive);
    }
}
