using System;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-ScrollItem")]
public class ScrollItem : MonoBehaviour
{
    [SerializeField] protected Button _button;
    [SerializeField] protected Text _text;

    private IDisposable onClickButtonObservable;

    public void SetText(string text)
    {
        _text.text = text;
    }

    public void SetOnClickAction(Action action) {
        if (action == null) return;

        if(onClickButtonObservable != null)
        {
            onClickButtonObservable.Dispose();
            onClickButtonObservable = null;
        }

        onClickButtonObservable = _button.OnClickIntentAsObservable()
            .Do(_ => action())
            .Subscribe();
    }
}