using System;
using System.Collections.Generic;
using GameBase;
using UniRx;

public class QuestCategoryWindowFactory
{
    public static IObservable<QuestCategoryWindowResponse> Create(QuestCategoryWindowRequest request)
    {
        return Observable.Create<QuestCategoryWindowResponse>(observer =>
        {
            var param = new Dictionary<string, object>();
            param.Add("onClose", new Action(() =>
            {
                observer.OnNext(new QuestCategoryWindowResponse());
                observer.OnCompleted();
            }));

            UIManager.Instance.OpenWindow<QuestCategoryWindowUIScript>(param, animationType: WindowAnimationType.None);
            return Disposable.Empty;
        });
    }
}

