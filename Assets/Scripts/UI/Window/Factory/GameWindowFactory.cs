using System;
using System.Collections.Generic;
using GameBase;
using UniRx;

public class GameWindowFactory
{
    public static IObservable<GameWindowResponse> Create(GameWindowRequest request)
    {
        return Observable.Create<GameWindowResponse>(observer =>
        {
            var param = new Dictionary<string, object>();
            UIManager.Instance.OpenWindow<GameWindowUIScript>(param, animationType: WindowAnimationType.None);
            return Disposable.Empty;
        });
    }
}