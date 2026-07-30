using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AdeebTask.Models;

namespace AdeebTask.UI.Screens
{
    public class ProjectCardView : MonoBehaviour
    {
        public event Action<string> OnCardClicked;

        [SerializeField] private TMP_Text _projectNameText;
        [SerializeField] private TMP_Text _lastModifiedText;
        [SerializeField] private RawImage _thumbnailImage;
        [SerializeField] private Button _cardButton;

        private string _projectId;

        private void Awake()
        {
            if (_cardButton != null)
            {
                _cardButton.onClick.AddListener(() => OnCardClicked?.Invoke(_projectId));
            }
        }

        private void OnDestroy()
        {
            if (_cardButton != null)
            {
                _cardButton.onClick.RemoveAllListeners();
            }
        }

        public void Setup(ProjectCardData data)
        {
            _projectId = data.projectId;

            if (_projectNameText != null)
                _projectNameText.text = data.projectName;

            if (_lastModifiedText != null)
            {
                // Format the unix timestamp
                var date = DateTimeOffset.FromUnixTimeSeconds(data.lastModifiedUtc).ToLocalTime().DateTime;
                _lastModifiedText.text = date.ToString("MMM dd, yyyy");
            }

            if (_thumbnailImage != null && !string.IsNullOrEmpty(data.thumbnailBase64))
            {
                SetThumbnail(data.thumbnailBase64);
            }
        }

        private void SetThumbnail(string base64String)
        {
            try
            {
                // Remove data:image/png;base64, prefix if present
                if (base64String.Contains(","))
                {
                    base64String = base64String.Split(',')[1];
                }

                byte[] imageBytes = Convert.FromBase64String(base64String);
                Texture2D tex = new Texture2D(2, 2);
                if (tex.LoadImage(imageBytes))
                {
                    _thumbnailImage.texture = tex;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProjectCardView] Failed to load thumbnail: {e.Message}");
            }
        }
    }
}
