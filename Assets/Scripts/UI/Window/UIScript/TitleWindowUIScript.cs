using System.Collections;
using System.Collections.Generic;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-Title")]
public class TitleWindowUIScript : WindowBase
{
    [SerializeField] protected Button _tapToStartButton;

    public override void Init(WindowInfo info)
    {
        _tapToStartButton.OnClickIntentAsObservable()
            .SelectMany(_ => ApiConnection.LoginWithCustomID())
            .SelectMany(res =>
            {
                if (res.NewlyCreated)
                {
                    // 新規作成
                }
                else
                {
                    // 通常ログイン
                    return Observable.ReturnUnit();
                }
                return Observable.ReturnUnit();
            })
            .Subscribe();
    }

    public override void Open(WindowInfo info)
    {
    }

    public override void Back(WindowInfo info)
    {
    }

    public override void Close(WindowInfo info)
    {
    }
}
