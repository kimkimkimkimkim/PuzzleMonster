using DG.Tweening;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace GameBase
{
    public abstract class DialogBase : MonoBehaviour
    {
        private const float MOVING_TIME = 0.2f;
        private const float FRAME_OUT_ANCHOR_POSITION = 1620.0f;
        private const float FRAME_IN_ANCHOR_POSOTION = 46.0f;
        private const float FRAME_INITIAL_POSOTION = 7.5f;
        private const float OPEN_LOCAL_SCALE = 0.95f;
        private const float CLOSE_LOCAL_SCALE = 0.7f;
        [SerializeField] protected GameObject _dialogFrame;
        [SerializeField] protected Image _grayOutImage;

        /// <summary>
        /// UIが生成された時に必ず一度だけ呼ばれます。
        /// </summary>
        /// <param name="info"></param>
        public abstract void Init(DialogInfo info);

        /// <summary>
        /// UIが画面にEnabledされた時、毎回呼ばれます。
        /// </summary>
        /// <param name="info"></param>
        public abstract void Open(DialogInfo info);

        /// <summary>
        /// UIが画面にDisabledされた時、毎回呼ばれます。
        /// </summary>
        /// <param name="info"></param>
        public abstract void Back(DialogInfo info);

        /// <summary>
        /// UIがDestroy時に一度呼ばれます。
        /// </summary>
        /// <param name="info"></param>
        public abstract void Close(DialogInfo info);

        public virtual void BackButton()
        {
            UIManager.Instance.CloseDialog();
        }

        public void PlayOpenAnimation(DialogAnimationType animationType)
        {
            if (_dialogFrame == null) return;
            var rect = _dialogFrame.GetComponent<RectTransform>();
            var canvas = _dialogFrame.GetComponent<CanvasGroup>();
            switch (animationType)
            {
                case DialogAnimationType.Center:
                    rect.localScale = rect.localScale * OPEN_LOCAL_SCALE;
                    rect.DOScale(Vector3.one, MOVING_TIME * 2).SetEase(Ease.OutBack);
                    canvas.alpha = 0.0f;
                    canvas.DOFade(1.0F, MOVING_TIME);
                    break;
                case DialogAnimationType.Right:
                    rect.position += new Vector3(FRAME_INITIAL_POSOTION, 0.0f, 0.0f);
                    rect.DOAnchorPosX(FRAME_IN_ANCHOR_POSOTION, MOVING_TIME);
                    break;
                case DialogAnimationType.Left:
                    rect.position += new Vector3(-FRAME_INITIAL_POSOTION, 0.0f, 0.0f);
                    rect.DOAnchorPosX(-FRAME_IN_ANCHOR_POSOTION, MOVING_TIME);
                    break;
                case DialogAnimationType.Bottom:
                    var initialPos = rect.localPosition;
                    rect.localPosition += new Vector3(0.0f, -Screen.height, 0.0f);
                    rect.DOLocalMove(initialPos, MOVING_TIME);
                    break;
            }
        }

        public IObservable<Unit> PlayCloseAnimationObservable(DialogAnimationType animationType)
        {
            if (_dialogFrame == null) return Observable.ReturnUnit();
            var rect = _dialogFrame.GetComponent<RectTransform>();
            var canvas = _dialogFrame.GetComponent<CanvasGroup>();
            var position = 0.0f;
            switch (animationType)
            {
                case DialogAnimationType.Center:
                    var scale = rect.localScale * CLOSE_LOCAL_SCALE;
                    rect.DOScale(scale, MOVING_TIME).SetEase(Ease.InOutBack);
                    canvas.DOFade(0.0F, MOVING_TIME);
                    return _grayOutImage.DOFade(0.0F, MOVING_TIME).OnCompleteAsObservable().AsUnitObservable();
                case DialogAnimationType.Right:
                    position = UIManager.Instance.dialogParant.position.x + FRAME_OUT_ANCHOR_POSITION;
                    return rect.DOAnchorPosX(position, MOVING_TIME).OnCompleteAsObservable().AsUnitObservable();
                case DialogAnimationType.Left:
                    position = UIManager.Instance.dialogParant.position.x - FRAME_OUT_ANCHOR_POSITION;
                    return rect.DOAnchorPosX(position, MOVING_TIME).OnCompleteAsObservable().AsUnitObservable();
                case DialogAnimationType.Bottom:
                    position = UIManager.Instance.dialogParant.position.y - FRAME_OUT_ANCHOR_POSITION;
                    return rect.DOAnchorPosY(position, MOVING_TIME).OnCompleteAsObservable().AsUnitObservable();
                case DialogAnimationType.None:
                default:
                    return Observable.ReturnUnit();
            }
        }
    }

    public enum DialogAnimationType
    {
        None = 0,
        Center = 1,
        Right = 2,
        Left = 3,
        Bottom = 4,
    }
}
