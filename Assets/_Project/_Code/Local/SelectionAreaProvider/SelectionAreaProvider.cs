using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Behaviours
{
    [DisallowMultipleComponent]
    public sealed class SelectionAreaProvider : MonoBehaviour, ISelectionAreaProvider,
        IPointerDownHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        public event Action<SelectionResult> OnSelectionResult;
        
        [Header("Area")]
        [SerializeField] private RectTransform _selectionArea;

        [Header("Visual")]
        [SerializeField] private RectTransform _selectionVisual;
        [SerializeField] private bool _hideVisualWhenIdle = true;

        [Header("Input")]
        [SerializeField] private PointerEventData.InputButton _requiredButton = PointerEventData.InputButton.Left;
        
        private bool _isDragging;
        private Vector2 _startLocalPoint;
        private Vector2 _currentLocalPoint;
        private Camera _uiCamera;

        [Serializable]
        public sealed class SelectionFinishedUnityEvent : UnityEvent<SelectionResult> { }

        private void Awake()
        {
            _selectionVisual.gameObject.SetActive(!_hideVisualWhenIdle);
            _selectionVisual.sizeDelta = Vector2.zero;
        }

        public bool InBounds(Vector2 screenPoint)
        {
            if (_selectionArea == null)
                return false;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_selectionArea, screenPoint, _uiCamera, out var localPoint))
                return false;
            return _selectionArea.rect.Contains(localPoint);
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != _requiredButton)
                return;
            if (!TryScreenToClampedLocal(eventData.position, eventData.pressEventCamera, out _startLocalPoint))
                return;
            
            _uiCamera = eventData.pressEventCamera;
            _currentLocalPoint = _startLocalPoint;
            _isDragging = true;

            UpdateVisual(_startLocalPoint, _currentLocalPoint);
            _selectionVisual.gameObject.SetActive(true);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_isDragging || eventData.button != _requiredButton)
                return;

            UpdateCurrentPoint(eventData);
            UpdateVisual(_startLocalPoint, _currentLocalPoint);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging || eventData.button != _requiredButton)
                return;

            UpdateCurrentPoint(eventData);
            UpdateVisual(_startLocalPoint, _currentLocalPoint);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging || eventData.button != _requiredButton)
                return;
            UpdateCurrentPoint(eventData);

            var result = BuildResult(_startLocalPoint, _currentLocalPoint, _uiCamera);

            _isDragging = false;
            if (_hideVisualWhenIdle)
                _selectionVisual.gameObject.SetActive(false);

            OnSelectionResult?.Invoke(result);
        }

        private void UpdateCurrentPoint(PointerEventData eventData)
        {
            if (!TryScreenToClampedLocal(eventData.position, _uiCamera, out _currentLocalPoint))
                _currentLocalPoint = _startLocalPoint;
        }

        private SelectionResult BuildResult(Vector2 localA, Vector2 localB, Camera uiCamera)
        {
            var localMin = Vector2.Min(localA, localB);
            var localMax = Vector2.Max(localA, localB);
            var screenA = RectTransformUtility.WorldToScreenPoint(uiCamera, _selectionArea.TransformPoint(localMin));
            var screenB = RectTransformUtility.WorldToScreenPoint(uiCamera, _selectionArea.TransformPoint(localMax));
            var screenMin = Vector2.Min(screenA, screenB);
            var screenMax = Vector2.Max(screenA, screenB);

            return new SelectionResult
            {
                LocalMin = localMin,
                LocalMax = localMax,
                ScreenMin = screenMin,
                ScreenMax = screenMax
            };
        }

        private bool TryScreenToClampedLocal(Vector2 screenPoint, Camera uiCamera, out Vector2 localPoint)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_selectionArea, screenPoint, uiCamera, out localPoint))
                return false;

            var rect = _selectionArea.rect;
            localPoint.x = Mathf.Clamp(localPoint.x, rect.xMin, rect.xMax);
            localPoint.y = Mathf.Clamp(localPoint.y, rect.yMin, rect.yMax);
            return true;
        }

        private void UpdateVisual(Vector2 localA, Vector2 localB)
        {
            var min = Vector2.Min(localA, localB);
            var max = Vector2.Max(localA, localB);
            var center = (min + max) * 0.5f;
            var size = max - min;
            _selectionVisual.anchoredPosition = center;
            _selectionVisual.sizeDelta = size;
        }
    }
}