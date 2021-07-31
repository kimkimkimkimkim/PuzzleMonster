using System;
using System.Collections.Generic;
using GameBase;
using UniRx;

public class QuestListWindowFactory
{
    public static IObservable<QuestListWindowResponse> Create(QuestListWindowRequest request)
    {
        return Observable.Create<QuestListWindowResponse>(observer =>
        {
            var param = new Dictionary<string, object>();
            param.Add("questCategoryId", request.questCategoryId);
            param.Add("onClose", new Action(() =>
            {
                observer.OnNext(new QuestListWindowResponse());
                observer.OnCompleted();
            }));

            UIManager.Instance.OpenWindow<QuestListWindowUIScript>(param, animationType: WindowAnimationType.None);
            return Disposable.Empty;
        });
    }
}

