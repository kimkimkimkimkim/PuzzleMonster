using System;
using System.Collections.Generic;
using GameBase;
using UniRx;

public class QuestSelectPartyWindowFactory
{
    public static IObservable<QuestSelectPartyWindowResponse> Create(QuestSelectPartyWindowRequest request)
    {
        return Observable.Create<QuestSelectPartyWindowResponse>(observer =>
        {
            var param = new Dictionary<string, object>();
            param.Add("questId", request.questId);
            param.Add("onClose", new Action(() =>
            {
                observer.OnNext(new QuestSelectPartyWindowResponse());
                observer.OnCompleted();
            }));

            UIManager.Instance.OpenWindow<QuestSelectPartyWindowUIScript>(param, animationType: WindowAnimationType.None);
            return Disposable.Empty;
        });
    }
}

