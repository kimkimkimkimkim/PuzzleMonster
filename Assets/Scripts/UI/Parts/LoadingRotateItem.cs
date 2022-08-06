using GameBase;
using UnityEngine;
using System.Collections.Generic;

[ResourcePath("UI/Parts/Parts-LoadingRotateItem")]
public class LoadingRotateItem : MonoBehaviour
{
    [SerializeField] protected List<CanvasGroup> _dotCanvasGroupList;

    private float roundTime = 1.5f;
    private float minAlpha = 0.2f;
    private float maxAlpha = 1.0f;
    private float minScale = 0.2f;
    private float maxScale = 1.0f;
    private float time = 0.0f;

    private void FixedUpdate()
    {
        time += Time.deltaTime;

        _dotCanvasGroupList.ForEach((canvasGroup, index) =>
        {
            var fixedTime = time + roundTime * ((float)index / (_dotCanvasGroupList.Count - 1));
            canvasGroup.alpha = minAlpha + (maxAlpha - minAlpha) * (roundTime - fixedTime % roundTime);
            canvasGroup.transform.localScale = Vector3.one * (minScale + (maxScale - minScale) * (roundTime - fixedTime % roundTime));
        });
    }
} 