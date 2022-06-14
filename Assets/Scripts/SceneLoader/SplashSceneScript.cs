using GameBase;
using UnityEngine;

public class SplashSceneScript : MonoBehaviour
{
    void Start()
    {
        SceneLoadManager.ChangeScene(SceneType.Title);
    }
}
