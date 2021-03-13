using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBase;
using UniRx;
using PlayFab;
using PlayFab.ClientModels;

public class MainSceneManager : SingletonMonoBehaviour<MainSceneManager>
{
    private void Start()
    {
        //GameWindowFactory.Create(new GameWindowRequest()).Subscribe();
        PlayFabClientAPI.LoginWithCustomID(
            new LoginWithCustomIDRequest { CustomId = "GettingStartedGuide", CreateAccount = true }
        , result => Debug.Log("おめでとうございます！ログイン成功です！")
        , error => Debug.Log("ログイン失敗...(´；ω；｀)"));
    }
}
