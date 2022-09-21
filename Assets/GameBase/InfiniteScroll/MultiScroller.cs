using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

namespace GameBase {
    public class MultiScroller : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler {
        private List<IDragHandler> parentDragHandlerList;
        private List<IBeginDragHandler> parentBeginDragHandlerList;
        private List<IEndDragHandler> parentEndDragHandlerList;

        private void Start() {
            parentDragHandlerList = GetComponentsInParent<IDragHandler>().Where(p => !(p is MultiScroller)).ToList();
            parentBeginDragHandlerList = GetComponentsInParent<IBeginDragHandler>().Where(p => !(p is MultiScroller)).ToList();
            parentEndDragHandlerList = GetComponentsInParent<IEndDragHandler>().Where(p => !(p is MultiScroller)).ToList();
        }

        public void OnDrag(PointerEventData pointerEventData) {
            parentDragHandlerList.ForEach(p => p.OnDrag(pointerEventData));
        }

        public void OnBeginDrag(PointerEventData pointerEventData) {
            parentBeginDragHandlerList.ForEach(p => p.OnBeginDrag(pointerEventData));
        }

        public void OnEndDrag(PointerEventData pointerEventData) {
            parentEndDragHandlerList.ForEach(p => p.OnEndDrag(pointerEventData));
        }
    }
}
