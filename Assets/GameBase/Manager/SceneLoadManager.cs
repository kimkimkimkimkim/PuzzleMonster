using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameBase
{
    public class SceneLoader
    {
        public static SceneType loadedSceneType { get; private set; }
        public static ISceneLoadable loadedController { get; private set; }

        private static AsyncOperation preloadScene;

        public static void Init()
        {
            // loadedController = new TitleLoader();
        }

        public static void ChangeScene(SceneType changeSceneType)
        {
            UIManager.Instance.DestroyAll();

            var param = loadedController != null ? loadedController.param : new Dictionary<string, object>();

            Observable.ObserveOnMainThread(DeactiveScene(param))
                .Do(_ => CommonLogger.Log("ChangeScene LoadScene: " + changeSceneType.ToString()))
                .SelectMany(_ => LoadSceneObservable(changeSceneType, loadedSceneType))
                .Do(_ =>
                {
                    loadedSceneType = changeSceneType;
                    loadedController = InstanciateScene();
                })
                .ObserveOnMainThread()

                .Do(_ => CommonLogger.Log("ChangeScene Unload: " + loadedSceneType.ToString()))
                .Do(_ =>
                {
                    GC.Collect();
                    Resources.UnloadUnusedAssets();
                })
                .ObserveOnMainThread()

                .Do(_ => CommonLogger.Log("ChangeScene Activate: " + loadedSceneType.ToString()))
                .SelectMany(_ => ActiveScene(param))
                .ObserveOnMainThread()

                .Do(_ => CommonLogger.Log("ChangeScene SetCamera: " + loadedSceneType.ToString()))
                // .Do(_ => CameraController.Instance.SetCameraMode(loadedSceneType))
                .ObserveOnMainThread()

                .Do(_ =>
                {
                    if (param.ContainsKey("dummyCanvas"))
                    {
                        /*
                        var dummyCanvas = (DummyCanvas)param["dummyCanvas"];
                        CommonLogger.Log("ChangeScene Destroy DummyCanvas: " + loadedSceneType.ToString());
                        GameObject.Destroy(dummyCanvas.gameObject);
                        */
                    }
                })

                .Do(_ => CommonLogger.Log("ChangeScene CleanUp: " + loadedSceneType.ToString()))
                .Subscribe(_ =>
                {
                    UIManager.Instance.TryHideLoadingView();
                    loadedController.OnLoadComplete();
                //if (ResourceManager.Instance.isInited && loadingUIScript != null)
                //GameObject.Destroy(loadingUIScript.gameObject);
            }, (ex) =>
            {
                CommonLogger.LogError("Error ChangeScene: " + ex.Message);
                UIManager.Instance.TryHideLoadingView();
                //ErrorChangeScene(ex, loadingUIScript);
            });
        }

        private static IObservable<Unit> DeactiveScene(Dictionary<string, object> param)
        {
            if (loadedController == null)
            {
                return Observable.Return(Unit.Default);
            }
            return loadedController.Deactivate(param);
        }

        private static IObservable<Unit> ActiveScene(Dictionary<string, object> param)
        {
            return loadedController.Activate(param);
        }

        private static ISceneLoadable InstanciateScene()
        {
            ISceneLoadable temp = null;

            switch (loadedSceneType)
            {
                case SceneType.Title:
                    // temp = new TitleLoader();
                    break;

                case SceneType.Tutorial:
                    //temp = new TutorialGameController();
                    break;

                case SceneType.Main:
                    // temp = new GardenGameController();
                    break;

                default:
                    CommonLogger.LogError("The new scene must have a SceneController");
                    break;
            }

            return temp;
        }

        private static void UnloadScene(SceneType unloadeSceneType)
        {
            SceneManager.UnloadSceneAsync(unloadeSceneType.ToString());
        }

        public static void PreloadScene(SceneType changeSceneType)
        {
            preloadScene = SceneManager.LoadSceneAsync(changeSceneType.ToString(), LoadSceneMode.Additive);
            preloadScene.allowSceneActivation = false;
        }

        private static IObservable<Unit> LoadSceneObservable(SceneType changeSceneType, SceneType loadedSceneType)
        {
            if (preloadScene != null)
            {
                // 事前ロード完了済(progress >= 0.9f)でシーンを有効化
                // https://docs.unity3d.com/jp/540/ScriptReference/AsyncOperation-allowSceneActivation.html
                // When allowSceneActivation is set to false then progress is stopped at 0.9
                if (preloadScene.progress >= 0.9f)
                {
                    preloadScene.allowSceneActivation = true;
                    preloadScene = null;
                    // 事前ロード利用時は前シーンを明示的に破棄
                    return Observable.NextFrame().Do(_ => UnloadScene(loadedSceneType));
                }
                else
                {
                    // 事前ロードが終わっていなかったら1フレーム待って再度確認
                    return Observable.NextFrame().SelectMany(_ => LoadSceneObservable(changeSceneType, loadedSceneType));
                }
            }
            else
            {
                // 事前ロードを利用しない場合通常読み込み
                // 通常読み込みの場合は自動で前シーンは破棄される
                SceneManager.LoadScene(changeSceneType.ToString());
                return Observable.ReturnUnit();
            }
        }
    }

    public abstract class ISceneLoadable
    {
        public Dictionary<string, object> param = new Dictionary<string, object>();

        public abstract IObservable<Unit> Activate(Dictionary<string, object> param);

        public abstract IObservable<Unit> Deactivate(Dictionary<string, object> param);

        public abstract void OnPause(bool pause);

        public abstract void OnLoadComplete();

        public virtual void OnLowMemoryAlert()
        {
        }
    }
}

public enum SceneType
{
    Splash,
    Title,
    Tutorial,
    Main,
}