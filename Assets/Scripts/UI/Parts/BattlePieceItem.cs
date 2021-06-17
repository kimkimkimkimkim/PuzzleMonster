using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattlePieceItem : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public void OnPointerDown(PointerEventData data)
    {
        Debug.Log("touch");
    }

    public void OnDrag(PointerEventData data)
    {
        var targetPos = Camera.main.ScreenToWorldPoint(data.position);
        targetPos.z = 0;
        transform.position = targetPos;
    }
}
