using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class BattleAnimationManager : MonoBehaviour
{
    [SerializeField] protected Transform _winOrLoseParent;

    public IObservable<Unit> PlayWinAnimation()
    {
        return Observable.ReturnUnit();
    }

    public IObservable<Unit> PlayLoseAnimation()
    {
        return Observable.ReturnUnit();
    }
}
