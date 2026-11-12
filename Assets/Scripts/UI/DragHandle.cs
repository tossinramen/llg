using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LLG
{
    /// <summary>Reports how far the mouse moves sideways while held down on this UI element.</summary>
    public class DragHandle : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        public event Action<float> Dragged;

        public void OnPointerDown(PointerEventData eventData) { }

        public void OnDrag(PointerEventData eventData) => Dragged?.Invoke(eventData.delta.x);
    }
}
