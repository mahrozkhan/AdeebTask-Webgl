using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AdeebTask.UI.Virtualization
{
    public abstract class VirtualScroller<TData, TView> : MonoBehaviour 
        where TView : MonoBehaviour, IVirtualScrollCell<TData>
    {
        [Header("Virtualization Settings")]
        [SerializeField] protected ScrollRect _scrollRect;
        [SerializeField] protected RectTransform _contentPanel;
        [SerializeField] protected TView _cellPrefab;
        
        [SerializeField] protected int _poolSize = 15;
        [SerializeField] protected float _cellHeight = 100f;
        [SerializeField] protected float _spacing = 10f;

        private List<TData> _currentData = new List<TData>();
        private List<TView> _activeCells = new List<TView>();
        private int _previousStartIndex = -1;
        private bool _isInitialized = false;

        protected virtual void Awake()
        {
            if (!_isInitialized) InitializePool();
        }

        protected virtual void OnEnable()
        {
            if (_scrollRect != null)
                _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }

        protected virtual void OnDisable()
        {
            if (_scrollRect != null)
                _scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
        }

        private void InitializePool()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                var cell = Instantiate(_cellPrefab, _contentPanel);
                cell.SetActive(false);
                
                var rect = cell.RectTransform;
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(1, 1);
                rect.pivot = new Vector2(0.5f, 1);
                
                _activeCells.Add(cell);
            }
            _isInitialized = true;
        }

        public void SetData(List<TData> data)
        {
            if (!_isInitialized) InitializePool();
            
            _currentData = data ?? new List<TData>();
            
            float totalHeight = _currentData.Count * (_cellHeight + _spacing);
            _contentPanel.sizeDelta = new Vector2(_contentPanel.sizeDelta.x, totalHeight);

            _scrollRect.verticalNormalizedPosition = 1f;
            _previousStartIndex = -1; 
            
            UpdateVisibleCells(0f);
        }

        private void OnScrollValueChanged(Vector2 scrollPos)
        {
            float contentY = Mathf.Max(0, _contentPanel.anchoredPosition.y); 
            UpdateVisibleCells(contentY);
        }

        private void UpdateVisibleCells(float contentY)
        {
            if (_currentData.Count == 0)
            {
                foreach (var cell in _activeCells) cell.SetActive(false);
                return;
            }

            int startIndex = Mathf.FloorToInt(contentY / (_cellHeight + _spacing));
            startIndex = Mathf.Clamp(startIndex, 0, Mathf.Max(0, _currentData.Count - _poolSize));

            if (startIndex == _previousStartIndex) return;
            _previousStartIndex = startIndex;

            for (int i = 0; i < _poolSize; i++)
            {
                int dataIndex = startIndex + i;
                var cell = _activeCells[i];

                if (dataIndex < _currentData.Count)
                {
                    cell.Bind(_currentData[dataIndex]);
                    cell.SetActive(true);
                    
                    float yPos = -(dataIndex * (_cellHeight + _spacing));
                    cell.RectTransform.anchoredPosition = new Vector2(0, yPos);
                }
                else
                {
                    cell.SetActive(false);
                }
            }
        }
    }
}
