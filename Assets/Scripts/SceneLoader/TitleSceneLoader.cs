using GameBase;
using System;
using System.Collections.Generic;
using UniRx;

public class TitleSceneLoader : ISceneLoadable
{
    public override IObservable<Unit> Activate(Dictionary<string, object> param)
    {
        return Observable.ReturnUnit();
    }

    public override IObservable<Unit> Deactivate(Dictionary<string, object> param)
    {
        return Observable.ReturnUnit();
    }

    public override void OnPause(bool pause)
    {
    }

    public override void OnLoadComplete()
    {
        TitleWindowFactory.Create(new TitleWindowRequest()).Subscribe();
    }
}