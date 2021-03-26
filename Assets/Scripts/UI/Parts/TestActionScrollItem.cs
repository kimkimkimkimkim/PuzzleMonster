using System;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-TestActionScrollItem")]
public class TestActionScrollItem : MonoBehaviour
{
    [SerializeField] protected Text _titleText;
    [SerializeField] protected Button _button;

    private IDisposable onClickActionObservable;

    public void SetTitle(string title)
    {
        _titleText.text = title;
    }

    public void SetOnClickAction(Action action)
    {
        if (action == null) return;

        if(onClickActionObservable != null)
        {
            onClickActionObservable.Dispose();
            onClickActionObservable = null;
        }

        onClickActionObservable = _button.OnClickIntentAsObservable()
            .Do(_ => action())
            .Subscribe();
    }
}