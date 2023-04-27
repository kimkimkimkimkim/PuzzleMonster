using GameBase;
using UnityEngine;

public class SplashSceneScript : MonoBehaviour
{
    private void Start()
    {
        SceneLoadManager.ChangeScene(SceneType.Title);
    }
}