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
        //PlayFabClientAPI.login
        TitleWindowFactory.Create(new TitleWindowRequest()).Subscribe();
    }
}
