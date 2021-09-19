using System;
using System.Collections.Generic;
using GameBase;
using UniRx;

public class QuestDetailWindowFactory
{
    public static IObservable<QuestDetailWindowResponse> Create(QuestDetailWindowRequest request)
    {
        return Observable.Create<QuestDetailWindowResponse>(observer =>
        {
            var param = new Dictionary<string, object>();
            param.Add("questId", request.questId);
            param.Add("onClose", new Action(() =>
            {
                observer.OnNext(new QuestDetailWindowResponse());
                observer.OnCompleted();
            }));

            UIManager.Instance.OpenWindow<QuestDetailWindowUIScript>(param, animationType: WindowAnimationType.None);
            return Disposable.Empty;
        });
    }
}

