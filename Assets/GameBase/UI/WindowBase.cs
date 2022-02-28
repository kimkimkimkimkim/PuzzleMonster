using DG.Tweening;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace GameBase
{
    public abstract class WindowBase : MonoBehaviour
    {
        // TODO 定義値をUnity上で操作できるようにする
        private const float MOVING_TIME = 0.2f;
        private const float FRAME_OUT_POSITION = 10.0f;

        public GameObject _windowFrame;
        public RectTransform _fullScreenBaseRT; // セーフエリアにかかわらず画面サイズで表示されるUI
        public Button _backButton;

        protected Action onClose;

        /// <summary>
        /// UIが生成された時に必ず一度だけ呼ばれます。
        /// </summary>
        /// <param name="info"></param>
        public virtual void Init(WindowInfo info) {
            onClose = (Action)info.param["onClose"];
            if(_backButton != null) { 
                _backButton.OnClickIntentAsObservable()
                    .Do(_ => UIManager.Instance.CloseWindow())
                    .Do(_ => {
                        if (onClose != null)
                        {
                            onClose();
                            onClose = null;
                        }
                    })
                    .Subscribe();
            }
        }

        /// <summary>
        /// UIが画面にEnabledされた時、毎回呼ばれます。
        /// </summary>
        /// <param name="info"></param>
        public abstract void Open(WindowInfo info);

        /// <summary>
        /// UIが画面にDisabledされた時、毎回呼ばれます。
        /// </summary>
        /// <param name="info"></param>
        public abstract void Back(WindowInfo info);

        /// <summary>
        /// UIがDestroy時に一度呼ばれます。
        /// </summary>
        /// <param name="info"></param>
        public virtual void Close(WindowInfo info)
        {
            if (onClose != null)
            {
                onClose();
                onClose = null;
            }
        }

        public virtual void BackButton()
        {
            if (BackButtonDisabled)
                return;

            UIManager.Instance.CloseWindow();
        }

        /// <summary>
        /// BackButtonの有効性
        /// true:無効にする、false:有効にする
        /// </summary>
        public bool BackButtonDisabled;

        public void PlayOpenAnimation(WindowAnimationType animationType)
        {
            if (_windowFrame == null) return;
            var rect = _windowFrame.GetComponent<RectTransform>();
            switch (animationType)
            {
                case WindowAnimationType.GardenWindow:
                    rect.position += new Vector3(0.0f, -FRAME_OUT_POSITION, 0.0f);
                    rect.DOLocalMoveY(0.0f, MOVING_TIME).SetEase(Ease.InOutQuart).SetUpdate(true);
                    break;
                case WindowAnimationType.FooterWindowRight:
                    rect.position += new Vector3(FRAME_OUT_POSITION, 0.0f, 0.0f);
                    rect.DOLocalMoveX(0.0f, MOVING_TIME).SetUpdate(true);
                    break;
                case WindowAnimationType.FooterWindowLeft:
                    rect.position += new Vector3(-FRAME_OUT_POSITION, 0.0f, 0.0f);
                    rect.DOLocalMoveX(0.0f, MOVING_TIME).SetUpdate(true);
                    break;
                case WindowAnimationType.None:
                default:
                    break;
            }
        }

        public IObservable<Unit> PlayCloseAnimationObservable(WindowAnimationType animationType)
        {
            if (_windowFrame == null) return Observable.ReturnUnit();
            var rect = _windowFrame.GetComponent<RectTransform>();
            var canvas = _windowFrame.GetComponent<CanvasGroup>();
            var position = 0.0f;
            switch (animationType)
            {
                case WindowAnimationType.GardenWindow:
                    position = UIManager.Instance.windowParent.position.y - FRAME_OUT_POSITION;
                    return rect.DOMoveY(position, MOVING_TIME).SetEase(Ease.InOutExpo).SetUpdate(true).OnCompleteAsObservable().AsUnitObservable();
                case WindowAnimationType.FooterWindowRight:
                    position = UIManager.Instance.windowParent.position.x + FRAME_OUT_POSITION;
                    return rect.DOMoveX(position, MOVING_TIME).SetUpdate(true).OnCompleteAsObservable().AsUnitObservable();
                case WindowAnimationType.FooterWindowLeft:
                    position = UIManager.Instance.windowParent.position.x - FRAME_OUT_POSITION;
                    return rect.DOMoveX(position, MOVING_TIME).SetUpdate(true).OnCompleteAsObservable().AsUnitObservable();
                case WindowAnimationType.None:
                default:
                    return Observable.ReturnUnit();
            }
        }
    }

    public enum WindowAnimationType
    {
        None = 0,
        GardenWindow = 1,
        FooterWindowRight = 2,
        FooterWindowLeft = 3,
    }
}