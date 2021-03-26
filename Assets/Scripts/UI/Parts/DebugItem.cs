using System;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-DebugItem")]
public class DebugItem : MonoBehaviour
{
    [SerializeField] protected Button _debugButton;

    private IDisposable onClickActionObservable;

    public void SetOnClickAction(Action action)
    {
        if (action == null) return;

        if(onClickActionObservable != null) {
            onClickActionObservable.Dispose();
            onClickActionObservable = null;
        }

        onClickActionObservable = _debugButton.OnClickIntentAsObservable()
            .Do(_ => action())
            .Subscribe();
    }
}