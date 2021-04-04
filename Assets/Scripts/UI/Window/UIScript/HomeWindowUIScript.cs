using System;
using System.Collections;
using System.Collections.Generic;
using PM.Enum.UI;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-Home")]
public class HomeWindowUIScript : WindowBase
{
    [SerializeField] protected Button _questButton;

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        _questButton.OnClickIntentAsObservable()
            .SelectMany(_ => QuestCategoryWindowFactory.Create(new QuestCategoryWindowRequest()))
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
        base.Close(info);
    }
}
