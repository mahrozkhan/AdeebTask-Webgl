using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AdeebTask.UI;

namespace AdeebTask.UI.Screens
{
    public class InitScreen : AppScreen
    {
        [SerializeField] private Slider _loadingBar;
        [SerializeField] private TMP_Text _loadingText;
        
        public void UpdateProgress(float progress, string message)
        {
            if (_loadingBar != null) _loadingBar.value = progress;
            if (_loadingText != null) _loadingText.text = message;
        }
    }
}
