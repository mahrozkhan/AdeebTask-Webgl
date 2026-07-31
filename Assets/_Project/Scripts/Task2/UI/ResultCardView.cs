using UnityEngine;
using TMPro;
using ContentDiscovery.Models;
using AdeebTask.UI.Virtualization;

namespace ContentDiscovery.UI
{
    public class ResultCardView : MonoBehaviour, IVirtualScrollCell<ContentItem>
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _authorText;
        [SerializeField] private TMP_Text _dateText;

        public RectTransform RectTransform { get; private set; }

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        public void Bind(ContentItem data)
        {
            if (data == null)
            {
                SetActive(false);
                return;
            }

            if (_titleText != null) _titleText.text = data.ContentName;
            if (_authorText != null) _authorText.text = data.Author;
            
            if (_dateText != null)
            {
                _dateText.text = data.Date.HasValue 
                    ? data.Date.Value.ToString("dd/MM/yyyy") 
                    : string.Empty;
            }
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
    }
}
