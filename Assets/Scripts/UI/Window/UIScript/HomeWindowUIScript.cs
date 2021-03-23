﻿using System;
using System.Collections;
using System.Collections.Generic;
using Enum.UI;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-Home")]
public class HomeWindowUIScript : WindowBase
{
    public override void Init(WindowInfo info)
    {
        Observable.Timer(TimeSpan.FromSeconds(2)).Do(_ => UIManager.Instance.TryHideFullScreenLoadingView()).Subscribe();
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