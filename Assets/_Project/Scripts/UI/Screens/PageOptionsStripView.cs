using System;
using UnityEngine;

namespace AdeebTask.UI.Screens
{
    public class PageOptionsStripView : MonoBehaviour
    {
        public event Action OnDeletePageClicked;

        public void HandleDeletePageClicked() => OnDeletePageClicked?.Invoke();
    }
}
