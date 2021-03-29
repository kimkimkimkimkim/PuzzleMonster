using System;
using System.Collections;
using System.Collections.Generic;
using PM.Enum.Battle;
using GameBase;
using UniRx;
using UnityEngine;

public class BattleAnimationManager : MonoBehaviour
{
    [SerializeField] protected GameObject _winObject;
    [SerializeField] protected GameObject _loseObject;

    public void Initialize()
    {
        _winObject.SetActive(false);
        _loseObject.SetActive(false);
    }

    public IObservable<Unit> PlayWinOrLoseAnimation(WinOrLose winOrLose)
    {
        if (winOrLose != WinOrLose.Win && winOrLose != WinOrLose.Lose) return Observable.ReturnUnit();

        var obj = winOrLose == WinOrLose.Win ? _winObject : _loseObject; 
        UIManager.Instance.ShowTapBlocker();
        obj.SetActive(true);

        return Observable.Timer(TimeSpan.FromSeconds(2))
            .Do(_ => UIManager.Instance.TryHideTapBlocker())
            .AsUnitObservable();
    }
}
