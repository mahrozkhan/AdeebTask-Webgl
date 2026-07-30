using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using AdeebTask.Models;

namespace AdeebTask.UI.Screens
{
    public class ColorSwatchView : MonoBehaviour
    {
        [SerializeField] private CanvasColorType _colorType;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Button _button;

        public event Action<CanvasColorType> OnSwatchClicked;

        private void Awake()
        {
            if (_label != null)
                _label.text = _colorType.ToString(); 

            if (_button != null)
                _button.onClick.AddListener(() => OnSwatchClicked?.Invoke(_colorType));
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveAllListeners();
        }
    }
}
