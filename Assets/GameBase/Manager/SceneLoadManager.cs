using GameBase;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace GameBase
{
    public class SceneLoadManager
    {
        public static SceneType activateSceneType { get; private set; } = SceneType.Splash;
        public static SceneType loadedSceneType { get; private set; } = SceneType.Splash;
        public static ISceneLoadable loadedController { get; private set; }
        public static ISceneLoadable previousLoadedController { get; private set; }

        private static AsyncOperation preloadScene;
        private static SceneType preloadSceneType;

        public static void ChangeScene(SceneType changeSceneType, bool isAdditive = false)
        {
            if (!isAdditive) UIManager.Instance.DestroyAll();

            var param = loadedController != null ? loadedController.param : new Dictionary<string, object>();

            Observable.ObserveOnMainThread(DeactiveScene(param, isAdditive))
                .Do(_ => CommonLogger.Log("ChangeScene LoadScene: " + changeSceneType.ToString()))
                .SelectMany(_ => LoadSceneObservable(changeSceneType, isAdditive))
                .Do(_ =>
                {
                    activateSceneType = changeSceneType;

                    previousLoadedController = loadedController;
                    loadedController = InstanciateScene(changeSceneType);
                })
                .ObserveOnMainThread()

                .Do(_ => CommonLogger.Log("ChangeScene Unload: " + loadedSceneType.ToString()))
                .SelectMany(_ => Resources.UnloadUnusedAssets().AsObservable())
                .Do(_ => GC.Collect())
                .ObserveOnMainThread()

                .Do(_ => CommonLogger.Log("ChangeScene Activate: " + changeSceneType.ToString()))
                .SelectMany(_ => ActiveScene(param))
                .ObserveOnMainThread()

                .Do(_ => CommonLogger.Log("ChangeScene SetCamera: " + changeSceneType.ToString()))
                // .Do(_ => CameraController.Instance.SetCameraMode(changeSceneType))
                .ObserveOnMainThread()

                .Do(_ =>
                {
                /*
                if (param.ContainsKey("dummyCanvas"))
                {
                    var dummyCanvas = (DummyCanvas)param["dummyCanvas"];

                    if (dummyCanvas != null)
                    {
                        GameObject.Destroy(dummyCanvas.gameObject);
                        KoiniwaLogger.Log("ChangeScene Destroy DummyCanvas: " + changeSceneType.ToString());
                    }

                    param.Remove("dummyCanvas");
                }
                */
                })

                .Do(_ => CommonLogger.Log("ChangeScene CleanUp: " + changeSceneType.ToString()))
                .Subscribe(_ =>
                {
                    if (!isAdditive) loadedSceneType = changeSceneType;
                    loadedController.OnLoadComplete();
                    UIManager.Instance.TryHideLoadingView();
                }, (ex) =>
                {
                    CommonLogger.LogError($"Error ChangeScene: {ex}");
                    UIManager.Instance.TryHideLoadingView();

                // 大概はAddressablesが原因なので、一旦リフレッシュする
                // ResourceManager.Instance.RefreshAddressables();

                // キャッシュが原因の場合もあるので、一旦キャッシュクリアする
                // ResourceManager.Instance.CacheClear();
                ErrorChangeScene();
                });
        }

        private static IObservable<Unit> DeactiveScene(Dictionary<string, object> param, bool isAdditive)
        {
            if (loadedController == null || isAdditive)
            {
                return Observable.ReturnUnit();
            }
            return loadedController.Deactivate(param);
        }

        private static IObservable<Unit> ActiveScene(Dictionary<string, object> param)
        {
            return loadedController.Activate(param);
        }

        private static ISceneLoadable InstanciateScene(SceneType changeSceneType)
        {
            ISceneLoadable sceneLoader = null;

            switch (changeSceneType)
            {
                case SceneType.Title:
                    sceneLoader = new TitleSceneLoader();
                    break;
                case SceneType.Tutorial:
                    sceneLoader = new TutorialSceneLoader();
                    break;
                case SceneType.Main:
                    sceneLoader = new MainSceneLoader();
                    break;
                default:
                    CommonLogger.LogError("The new scene must have a SceneController");
                    break;
            }

            return sceneLoader;
        }

        public static void PreloadScene(SceneType changeSceneType)
        {
            preloadScene = SceneManager.LoadSceneAsync(changeSceneType.ToString());
            preloadScene.allowSceneActivation = false;
            preloadSceneType = changeSceneType;
        }

        private static IObservable<Unit> LoadSceneObservable(SceneType changeSceneType, bool isAdditive = false)
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

                    if (preloadSceneType != changeSceneType)
                    {
                        // 事前ロードしたシーンが遷移したいシーンではなかった場合は遷移したいシーンを再読込
                        return LoadSceneObservable(changeSceneType, isAdditive);
                    }
                    else
                    {
                        return Observable.ReturnUnit();
                    }
                }
                else
                {
                    // 事前ロードが終わっていなかったら1フレーム待って再度確認
                    return Observable.NextFrame().SelectMany(_ => LoadSceneObservable(changeSceneType, isAdditive));
                }
            }
            else
            {
                // 事前ロードを利用しない場合通常読み込み
                var loadSceneMode = isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single;
                SceneManager.LoadScene(changeSceneType.ToString(), loadSceneMode);
                return Observable.ReturnUnit();
            }
        }

        private static void ErrorChangeScene()
        {
            // TODO 適切なエラーメッセージ
            CommonDialogFactory.Create(new CommonDialogRequest()
            {
                commonDialogType = PM.Enum.UI.CommonDialogType.YesOnly,
                title = "エラー",
                content = "シーンの遷移に失敗しました。\nタイトルに戻ります。",
            })
                .Do(_ => ChangeScene(SceneType.Title))
                .Subscribe();
        }

        /// <summary>
        /// マルチシーンで呼び出したシーンを破棄する。loadedControllerは元のシーンのものに戻す
        /// </summary>
        public static IObservable<Unit> UnLoadAdditiveSceneAsObservable(SceneType unLoadSceneType)
        {
            if (previousLoadedController == null)
            {
                CommonLogger.Log("not found additive scene.");
                return Observable.ReturnUnit();
            }

            var param = loadedController != null ? loadedController.param : new Dictionary<string, object>();
            return DeactiveScene(param, false)
                .SelectMany(_ => SceneManager.UnloadSceneAsync(unLoadSceneType.ToString()).AsObservable())
                .Do(_ =>
                {
                    loadedController = previousLoadedController;
                    previousLoadedController = null;
                })
                .AsUnitObservable();
        }
    }

    public abstract class ISceneLoadable: MonoBehaviour
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