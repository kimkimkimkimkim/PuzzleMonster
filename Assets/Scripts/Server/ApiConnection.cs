﻿using System;
using PlayFab;
using PlayFab.ClientModels;
using UniRx;
using UnityEngine;

public class ApiConnection
{
    /// <summary>
    /// カスタムIDでのログイン処理
    /// </summary>
    public static IObservable<LoginResult> LoginWithCustomID()
    {
        return Observable.Create<LoginResult>(o =>
        {
            var callback = new Action<LoginResult>(res =>
            {
                o.OnNext(res);
                o.OnCompleted();
            });
            var onErrorAction = new Action<PlayFabError>(error => 
            {
                OnErrorAction(error);
                o.OnCompleted();
            });

            PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                CustomId = SaveDataUtil.System.GetCustomId(),
                CreateAccount = true,
            },res => callback(res),error => onErrorAction(error));
            return Disposable.Empty;
        });
    }

    private static void OnErrorAction(PlayFabError error)
    {

    }
}
