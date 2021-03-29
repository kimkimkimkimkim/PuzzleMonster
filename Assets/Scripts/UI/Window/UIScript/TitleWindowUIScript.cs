using System;
using System.Collections;
using System.Collections.Generic;
using PM.Enum.UI;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-Title")]
public class TitleWindowUIScript : MonoBehaviour
{
    [SerializeField] protected GameObject _tapToStartButtonBase;
    [SerializeField] protected Button _tapToStartButton;

    private IDisposable _onClickButtonObservable;

    public void ShowTapToStartButton(bool isShow)
    {
        _tapToStartButtonBase.SetActive(isShow);
    }

    public void SetOnClickAction(Action action)
    {
        if (action == null) return;

        if(_onClickButtonObservable != null)
        {
            _onClickButtonObservable.Dispose();
            _onClickButtonObservable = null;
        }

        _onClickButtonObservable = _tapToStartButton.OnClickIntentAsObservable()
            .Do(_ => action())
            .Subscribe();
    }
}
