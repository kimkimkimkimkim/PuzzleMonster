using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using GameBase;

namespace GameBase
{
    public class UIManager : SingletonMonoBehaviour<UIManager>
    {
        public const int FIXED_RES_HEIGHT = 1920;
        public const int FIXED_RES_WIDTH = 1080;

        public Transform canvas;

        public Transform dialogParant;

        public Transform windowParent;

        public Transform propertyParent;

        public Transform toastParent;

        public Transform tutorialParent;

        public Transform debugParent;

        public float screenScale;

        public bool isUnlockAllUI;

        public bool isSkipTutorial;

        public float ScaledDpi
        {
            get
            {
                const float DUMMY_DPI = 520f;//DPIがとれないときのため
                var dpi = Screen.dpi > 0 ? Screen.dpi : DUMMY_DPI;
                return dpi / screenScale;
            }
        }
        public string deviceToken { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            if (base.IsDestroyed()) return;

            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(windowParent.root.gameObject);

            screenScale = (float)Screen.height / (float)FIXED_RES_HEIGHT;

            //InputSource.isEnabled = true;
        }

        #region TapBlocker

        private int _tapBlockerCallStackCount = 0;

        public GameObject _tapBlocker;

        /// <summary>
        /// TapBlockerのStackを増加させ、タップブロッカーを表す。
        /// </summary>
        public void ShowTapBlocker()
        {
            _tapBlockerCallStackCount += 1;
            _tapBlocker.SetActive(true);
            _tapBlocker.transform.SetAsLastSibling();
        }

        /// <summary>
        /// TapBlockerのStackを減少させ、０になったときはタップブロッカーを隠す。
        /// </summary>
        public void TryHideTapBlocker()
        {
            //MEMO：強制的に_tapBlockerCallStackCountを0に変更したりするなど、
            //Stackを無視してTapBlockerが隠すなどの処理を絶対に追加しないこと。
            _tapBlockerCallStackCount -= 1;
            if (_tapBlocker != null && _tapBlockerCallStackCount <= 0)
            {
                _tapBlockerCallStackCount = 0;
                _tapBlocker.SetActive(false);
            }
        }

        #endregion TapBlocker

        #region Current UI Info

        public WindowInfo currentWindowInfo { get; set; }

        public DialogInfo currentDialogInfo { get; set; }

        public bool isVisibleUiAtLast
        {
            get
            {
                return (isWindowAtLast || isDialogAtLast);
            }
        }

        [NonSerialized] public bool isWindowAtLast;

        [NonSerialized] public bool isDialogAtLast;

        #endregion Current UI Info

        #region Window Method

        public T OpenWindow<T>(Dictionary<string, object> param = null, bool previousActive = false, WindowAnimationType animationType = WindowAnimationType.GardenWindow) where T : WindowBase
        {

            /*
            if (param != null && param.ContainsKey("propertyGroupType")) {
                var propertyGroupType = (PropertyGroupType)param["propertyGroupType"];
                PropertyManager.Instance.SetPropertyGroup(propertyGroupType);
            } else {
                PropertyManager.Instance.SetPropertyGroup(PropertyGroupType.None);
            }
            GardenManager.Instance.ShowOffPanels();
            if (currentWindowInfo != null) {
                isWindowAtLast = true;
                InputSource.isEnabled = !isVisibleUiAtLast;
            }
            */

            T currentUIBase = CreateContent<T>();

            GameObject obj = currentUIBase.gameObject;
            obj.transform.SetAsFirstSibling();

            WindowInfo createdStateInfo = new WindowInfo();
            createdStateInfo.component = currentUIBase;
            createdStateInfo.param = param;

            currentUIBase.PlayOpenAnimation(animationType);

            if (currentWindowInfo == null)
            {
                //make head
                createdStateInfo.parent = null;
                currentWindowInfo = createdStateInfo;
            }
            else
            {
                currentWindowInfo.child = createdStateInfo;
                createdStateInfo.parent = currentWindowInfo;

                currentWindowInfo.component.Back(currentWindowInfo);
                currentWindowInfo.component.gameObject.SetActive(previousActive);

                currentWindowInfo = createdStateInfo;
            }

            currentUIBase.Init(createdStateInfo);
            currentUIBase.Open(createdStateInfo);

            return currentUIBase;
        }

        public void OpenWindowAsync<T>(Dictionary<string, object> param = null, bool previousActive = false, Action<T> callback = null) where T : WindowBase
        {
            /*
            if (param != null && param.ContainsKey("propertyGroupType")) {
                var propertyGroupType = (PropertyGroupType)param["propertyGroupType"];
                PropertyManager.Instance.SetPropertyGroup(propertyGroupType);
            } else {
                PropertyManager.Instance.SetPropertyGroup(PropertyGroupType.None);
            }
            */

            if (currentWindowInfo != null)
            {
                isWindowAtLast = true;
                //InputSource.isEnabled = !isVisibleUiAtLast;
            }

            ShowTapBlocker();

            CreateContentAsync<T>(null, (loadedWindow) =>
            {
                GameObject obj = loadedWindow.gameObject;
                obj.transform.SetAsFirstSibling();

                WindowInfo createdStateInfo = new WindowInfo();
                createdStateInfo.component = loadedWindow;
                createdStateInfo.param = param;

                if (currentWindowInfo == null)
                {
                    //make head
                    createdStateInfo.parent = null;
                    currentWindowInfo = createdStateInfo;
                }
                else
                {
                    currentWindowInfo.child = createdStateInfo;
                    createdStateInfo.parent = currentWindowInfo;

                    currentWindowInfo.component.Back(currentWindowInfo);
                    currentWindowInfo.component.gameObject.SetActive(previousActive);

                    currentWindowInfo = createdStateInfo;
                }

                loadedWindow.Init(createdStateInfo);
                loadedWindow.Open(createdStateInfo);

                TryHideTapBlocker();

                if (callback != null)
                {
                    callback(loadedWindow);
                }
            });
        }

        public T StackWindow<T>(Dictionary<string, object> param = null, bool previousActive = false) where T : WindowBase
        {
            if (currentWindowInfo != null)
            {
                isWindowAtLast = true;
                //InputSource.isEnabled = !isVisibleUiAtLast;
            }

            T currentUIBase = CreateContent<T>();

            GameObject obj = currentUIBase.gameObject;
            obj.transform.SetAsFirstSibling();

            WindowInfo createdStateInfo = new WindowInfo();
            createdStateInfo.component = currentUIBase;
            createdStateInfo.param = param;

            if (currentWindowInfo == null)
            {
                //make head
                createdStateInfo.parent = null;
                currentWindowInfo = createdStateInfo;
            }
            else
            {
                currentWindowInfo.child = createdStateInfo;
                createdStateInfo.parent = currentWindowInfo;

                currentWindowInfo.component.Back(currentWindowInfo);
                currentWindowInfo.component.gameObject.SetActive(previousActive);

                currentWindowInfo = createdStateInfo;
            }

            //Openはせずに初期化とスタックのみする
            currentUIBase.Init(createdStateInfo);

            return currentUIBase;
        }

        public void CloseWindow(bool openLastWindow = true, bool forceCloseParent = false, WindowAnimationType animationType = WindowAnimationType.GardenWindow)
        {
            if (currentWindowInfo == null || (currentWindowInfo.parent == null && forceCloseParent == false))
            {
                return;
            }

            var parentContrentStateInfo = currentWindowInfo.parent;

            currentWindowInfo.component.Close(currentWindowInfo);

            var closeObject = currentWindowInfo.component.gameObject;
            currentWindowInfo.component.PlayCloseAnimationObservable(animationType)
                .Do(_ => Destroy(closeObject))
                .Subscribe();

            currentWindowInfo.parent = null;
            currentWindowInfo.child = null;

            if (parentContrentStateInfo != null)
            {
                currentWindowInfo = parentContrentStateInfo;
                currentWindowInfo.child = null;
                currentWindowInfo.component.gameObject.SetActive(true);

                if (openLastWindow) currentWindowInfo.component.Open(currentWindowInfo);

                if (currentWindowInfo.parent == null)
                {
                    isWindowAtLast = false;
                    //InputSource.isEnabled = !isVisibleUiAtLast;
                }
            }
            else
            {
                currentWindowInfo = null;
                isWindowAtLast = false;
                //InputSource.isEnabled = !isVisibleUiAtLast;
            }
        }

        public void CloseAllWindow(bool isForce = false, WindowAnimationType animationType = WindowAnimationType.None)
        {
            if (isForce)
                while (currentWindowInfo != null)
                    CloseWindow(true, true, animationType);

            if (currentWindowInfo == null || currentWindowInfo.parent == null)
            {
                return;
            }

            while (isWindowAtLast)
            {
                var isLast = (currentWindowInfo.parent != null && currentWindowInfo.parent.parent == null);
                CloseWindow(isLast, false, animationType);
            }
        }

        /// <summary>
        /// Closeアクションを必ず次のチェーン処理の前に呼び出すために1フレームの遅延を挟むObservable
        /// </summary>
        public IObservable<Unit> CloseWindowObservable(WindowAnimationType animationType = WindowAnimationType.GardenWindow)
        {
            return Observable.ReturnUnit()
                .Do(_ => CloseWindow(true, false, animationType))
                .SelectMany(_ => Observable.NextFrame());
        }
        #endregion Window Method

        #region Popup Method

        public T OpenDialog<T>(Dictionary<string, object> param = null, bool displayingOnDialog = false, DialogAnimationType animationType = DialogAnimationType.Center) where T : DialogBase
        {
            /*
            if (param != null && param.ContainsKey("propertyGroupType")) {
                var propertyGroupType = (PropertyGroupType)param["propertyGroupType"];
                PropertyManager.Instance.SetPropertyGroup(propertyGroupType);
            } else {
                PropertyManager.Instance.SetPropertyGroup(PropertyGroupType.None);
            }
            */

            isDialogAtLast = true;
            //InputSource.isEnabled = !isVisibleUiAtLast;

            T currentUIBase = CreateContent<T>(dialogParant);
            GameObject obj = currentUIBase.gameObject;
            obj.transform.SetAsLastSibling();

            DialogInfo createdStateInfo = new DialogInfo();
            createdStateInfo.component = currentUIBase;
            createdStateInfo.param = param;

            if (currentDialogInfo == null)
            {
                //make head
                createdStateInfo.parent = null;
                currentDialogInfo = createdStateInfo;
            }
            else
            {
                currentDialogInfo.child = createdStateInfo;
                createdStateInfo.parent = currentDialogInfo;

                currentDialogInfo.component.Back(currentDialogInfo);
                currentDialogInfo.component.gameObject.SetActive(displayingOnDialog);

                currentDialogInfo = createdStateInfo;
            }

            currentDialogInfo.animationType = animationType;
            currentUIBase.PlayOpenAnimation(animationType);
            currentUIBase.Init(createdStateInfo);
            currentUIBase.Open(createdStateInfo);

            return currentUIBase;
        }

        public void OpenDialogAsync<T>(Dictionary<string, object> param = null, bool displayingOnDialog = false, Action<T> callback = null) where T : DialogBase
        {
            ShowTapBlocker();
            isDialogAtLast = true;
            //InputSource.isEnabled = !isVisibleUiAtLast;

            CreateContentAsync<T>(dialogParant, loadedDialog =>
            {
                GameObject obj = loadedDialog.gameObject;
                obj.transform.SetAsLastSibling();

                DialogInfo createdStateInfo = new DialogInfo();
                createdStateInfo.component = loadedDialog;
                createdStateInfo.param = param;

                if (currentDialogInfo == null)
                {
                    //make head
                    createdStateInfo.parent = null;
                    currentDialogInfo = createdStateInfo;
                }
                else
                {
                    currentDialogInfo.child = createdStateInfo;
                    createdStateInfo.parent = currentDialogInfo;

                    currentDialogInfo.component.Back(currentDialogInfo);
                    currentDialogInfo.component.gameObject.SetActive(displayingOnDialog);

                    currentDialogInfo = createdStateInfo;
                }

                loadedDialog.Init(createdStateInfo);
                loadedDialog.Open(createdStateInfo);

                TryHideTapBlocker();

                if (callback != null)
                {
                    callback(loadedDialog);
                }
            });
        }

        public void CloseDialog(bool isForce = false)
        {

            if (currentDialogInfo == null)
            {
                return;
            }

            var parentContrentStateInfo = currentDialogInfo.parent;
            currentDialogInfo.component.Close(currentDialogInfo);

            // ダイアログアニメーション
            var closeObject = currentDialogInfo.component.gameObject;
            if (isForce == false)
            {
                currentDialogInfo.component.PlayCloseAnimationObservable(currentDialogInfo.animationType)
                    .Do(_ => Destroy(closeObject))
                    .Subscribe();
            }
            else
            {
                Destroy(closeObject);
            }

            currentDialogInfo.parent = null;
            currentDialogInfo.child = null;

            if (parentContrentStateInfo != null)
            {
                currentDialogInfo = parentContrentStateInfo;
                currentDialogInfo.child = null;
                currentDialogInfo.component.gameObject.SetActive(true);
                currentDialogInfo.component.Open(currentDialogInfo);
            }
            else
            {
                currentDialogInfo = null;
            }

            // プロパティアニメーション
            /*
            if (isForce == false) {
                var propertyType = PropertyGroupType.None;
                if (parentContrentStateInfo != null) {
                    if (currentDialogInfo.param != null && currentDialogInfo.param.ContainsKey("propertyGroupType")) propertyType = (PropertyGroupType)currentDialogInfo.param["propertyGroupType"];
                } else {
                    if (currentWindowInfo != null) {
                        if (currentWindowInfo.param != null && currentWindowInfo.param.ContainsKey("propertyGroupType")) propertyType = (PropertyGroupType)currentWindowInfo.param["propertyGroupType"];
                    } else {
                        propertyType = PropertyGroupType.CloverHeartCoinOrb;
                    }
                }
                PropertyManager.Instance.SetPropertyGroup(propertyType);
            }
            */

            if (currentDialogInfo == null)
            {
                isDialogAtLast = false;
                //InputSource.isEnabled = !isVisibleUiAtLast;
            }
        }

        /// <summary>
        /// Closeアクションを必ず次のチェーン処理の前に呼び出すために1フレームの遅延を挟むObservable
        /// </summary>
        public IObservable<Unit> CloseDialogObservable()
        {
            return Observable.ReturnUnit()
                .Do(_ => CloseDialog())
                .SelectMany(_ => Observable.NextFrame());
        }

        #region PopupExtension Method
        /*
        public void ShowInformationDialog(string title, string content, Action onClickScreen = null, bool displayingOnDialog = false) {
            Dictionary<string, object> param = new Dictionary<string, object>();

            param.Add("info", CommonDialogInfo.DialogType.Information);
            param.Add("title", title);
            param.Add("content", content);
            param.Add("one", onClickScreen);

            OpenDialog<CommonDialogUIScript>(param, displayingOnDialog);
        }

        public void ShowOneButtonDialog(string title, string content, Action onClickOKButton = null, bool displayingOnDialog = false, string buttonText = "") {
            Dictionary<string, object> param = new Dictionary<string, object>();

            param.Add("info", CommonDialogInfo.DialogType.Information);
            param.Add("title", title);
            param.Add("content", content);
            param.Add("one", onClickOKButton);
            if (string.IsNullOrEmpty(buttonText)) {
                buttonText = ApplicationContext.LocaledTextResource.GetString("[OK]");
            }
            param.Add("oneButtonText", buttonText);
            //OpenDialog<CommonDialogUIScript>(param, displayingOnDialog);
            OpenDialogAsync<CommonDialogUIScript>(param, displayingOnDialog);

        }
        */
        #endregion PopupExtension Method

        #endregion Popup Method

        public T CreateContent<T>(Transform parent = null) where T : MonoBehaviour
        {
            ResourcePathAttribute attr = (ResourcePathAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(ResourcePathAttribute));
#if UNITY_EDITOR
            if (attr == null)
            {
                Debug.LogError("Error :: " + typeof(T).Name);
            }
#endif
            var path = attr.resourcePath;

            GameObject tmp = (GameObject)ResourceManager.Instance.LoadAsset<GameObject>(path);

            if (parent == null) parent = windowParent;
            var obj = Instantiate(tmp, parent);
            obj.name = tmp.name;

            T component = null;

            if (obj != null)
            {
                component = obj.GetComponent<T>();
            }

#if UNITY_EDITOR
            if (obj == null || component == null)
            {
                Debug.LogError("UI Manager load failure :: " + typeof(T).Name);
                return null;
            }
#endif
            return component;
        }

        public void CreateContentAsync<T>(Transform parent, Action<T> callback = null, Func<bool> isContinuableCallback = null) where T : MonoBehaviour
        {
            ResourcePathAttribute attr = (ResourcePathAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(ResourcePathAttribute));
#if UNITY_EDITOR
            if (attr == null)
            {
                Debug.LogError("Error :: " + typeof(T).Name);
                return;
            }
#endif
            var path = attr.resourcePath;
            T component = null;

            ResourceManager.Instance.LoadAssetAsyncObservable<GameObject>(path)
                .CatchIgnore((Exception e) => Debug.LogError(e))
                .Do(loadedObj =>
                {
                    if (loadedObj != null)
                    {
                        if (parent == null) return;
                        if (isContinuableCallback != null)
                        {
                            if (!isContinuableCallback()) return;
                        }
                        var obj = Instantiate(loadedObj, parent);
                        if (obj != null)
                        {
                            obj.name = loadedObj.name;

                            component = obj.GetComponent<T>();
                        }

#if UNITY_EDITOR
                        if (obj == null || component == null)
                        {
                            Debug.LogError("UI Manager load failure :: " + typeof(T).Name);
                        }
#endif
                    }
                })
                .CatchIgnore((Exception e) => Debug.LogError(e))
                .Do(_ =>
                {
                    if ((callback != null) && (component != null))
                    {
                        callback(component);
                    }
                })
                .Subscribe(_ => { }, (Exception ex) => Debug.LogError(ex));
        }

        public T LoadPrefab<T>() where T : MonoBehaviour
        {
            ResourcePathAttribute attr = (ResourcePathAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(ResourcePathAttribute));
#if UNITY_EDITOR
            if (attr == null)
            {
                Debug.LogError("Error :: " + typeof(T).Name);
            }
#endif
            var path = attr.resourcePath;

            GameObject tmp = ResourceManager.Instance.LoadAsset<GameObject>(path);
            T component = null;

            if (tmp != null)
            {
                component = tmp.GetComponent<T>();
            }

#if UNITY_EDITOR
            if (tmp == null || component == null)
            {
                Debug.LogError("UI Manager load failure :: " + typeof(T).Name);
            }
#endif
            return component;
        }

        public void DestroyAll()
        {
            CloseDialog(true);
            CloseAllWindow(true);
        }

        public void SetWindowBlocksRayCasts(bool isOn)
        {
            if (windowParent != null)
            {
                var canvasGroup = windowParent.gameObject.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.blocksRaycasts = isOn;
                }
            }
        }
    }


    #region UI Info Class

    public class WindowInfo
    {
        public WindowInfo parent;

        public WindowInfo child;

        public WindowBase component;

        public Dictionary<string, object> param;

        public WindowInfo FindParent<T>()
        {
            var temp = this;

            while (temp != null)
            {
                if (temp.component is T)
                {
                    break;
                }
                else
                {
                    temp = temp.parent;
                }
            }

            return temp;
        }

        public void DestoryRecursively()
        {
            if (child != null)
            {
                child.DestoryRecursively();
            }

            UIManager.Instance.currentWindowInfo = parent;

            if (parent == null)
            {
                UIManager.Instance.isWindowAtLast = false;
                //InputSource.isEnabled = !UIManager.Instance.isVisibleUiAtLast;
            }

            if (component != null)
            {
                component.Back(this);
                component.Close(this);
                parent = null;
                child = null;

                GameObject.Destroy(component.gameObject);
            }
        }
    }

    public class DialogInfo
    {
        public DialogInfo parent;

        public DialogInfo child;

        public DialogBase component;

        public Dictionary<string, object> param;

        public DialogAnimationType animationType;

        public DialogInfo FindParent<T>()
        {
            var temp = this;

            while (temp != null)
            {
                if (temp.component is T)
                {
                    break;
                }
                else
                {
                    temp = temp.parent;
                }
            }

            return temp;
        }

        public void DestoryRecursively()
        {
            if (child != null)
            {
                child.DestoryRecursively();
            }

            if (parent == null)
            {
                UIManager.Instance.isDialogAtLast = false;
                //InputSource.isEnabled = !UIManager.Instance.isVisibleUiAtLast;
            }

            UIManager.Instance.currentDialogInfo = parent;

            if (component != null)
            {
                component.Back(this);
                component.Close(this);

                parent = null;
                child = null;

                GameObject.Destroy(component.gameObject);
            }
        }
    }
    #endregion UI Info Class
}