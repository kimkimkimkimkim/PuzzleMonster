using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBase;
using UniRx;

public class MainSceneManager : SingletonMonoBehaviour<MainSceneManager>
{
    private void Start()
    {
        GameWindowFactory.Create(new GameWindowRequest()).Subscribe();
    }
}
