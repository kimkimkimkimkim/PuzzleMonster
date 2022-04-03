using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using DG.Tweening;

public class MonsterStrengthFxDialogBaseUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected GameObject _titleTextBase;
    [SerializeField] protected GameObject _titleTextComponent;
    [SerializeField] protected GameObject _contentBase;

    private void Awake()
    {
        _titleTextBase.SetActive(false);
        _titleTextComponent.SetActive(false);
        _contentBase.SetActive(false);
    }
    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action)info.param["onClickClose"];

        _closeButton.OnClickIntentAsObservable()
            .SelectMany(_ => UIManager.Instance.CloseDialogObservable())
            .Do(_ => {
                if (onClickClose != null)
                {
                    onClickClose();
                    onClickClose = null;
                }
            })
            .Subscribe();
    }

    protected IObservable<Unit> PlayAnimationObservable()
    {
        var TITLE_TEXT_BASE_FROM_SCALE = new Vector3(1.0f, 0.0f, 1.0f);
        var TITLE_TEXT_BASE_TO_SCALE = Vector3.one;
        var TITLE_TEXT_COMPONENT_FROM_SCALE = new Vector3(1.0f, 0.0f, 1.0f);
        var TITLE_TEXT_COMPONENT_TO_SCALE = Vector3.one;
        var TITLE_TEXT_BASE_FROM_POSITION = Vector3.up * 100.0f;
        var TITLE_TEXT_BASE_TO_POSITION = Vector3.up * 170.0f;
        var CONTENT_BASE_INITIAL_POSITION = Vector3.up * -80.0f;

        const float TITLE_TEXT_BASE_SCALE_ANIMATION_DELAY = 0.0f;
        const float TITLE_TEXT_BASE_SCALE_ANIMATION_TIME = 1.2f;
        const Ease TITLE_TEXT_BASE_SCALE_ANIMATION_EASE = Ease.OutExpo;
        const float TITLE_TEXT_COMPONENT_ANIMATION_DELAY = 0.2f;
        const float TITLE_TEXT_COMPONENT_ANIMATION_TIME = 1.2f;
        const Ease TITLE_TEXT_COMPONENT_ANIMATION_EASE = Ease.OutExpo;
        const float TITLE_TEXT_BASE_MOVE_ANIMATION_DELAY = TITLE_TEXT_COMPONENT_ANIMATION_DELAY + TITLE_TEXT_COMPONENT_ANIMATION_TIME + 0.3f;
        const float TITLE_TEXT_BASE_MOVE_ANIMATION_TIME = 0.2f;
        const float CONTENT_BASE_ANIMATION_DELAY = TITLE_TEXT_BASE_MOVE_ANIMATION_DELAY;
        const float LAST_DELAY = 1.5f;

        return Observable.ReturnUnit()
            .Do(_ => UIManager.Instance.ShowTapBlocker())
            .SelectMany(_ =>
            {
                var titleTextBaseScaleAnimationSequence = DOTween.Sequence()
                    .SetDelay(TITLE_TEXT_BASE_SCALE_ANIMATION_DELAY)
                    .AppendCallback(() =>
                    {
                        _titleTextBase.transform.localScale = TITLE_TEXT_BASE_FROM_SCALE;
                        _titleTextBase.transform.localPosition = TITLE_TEXT_BASE_FROM_POSITION;
                        _titleTextBase.SetActive(true);
                    })
                    .Append(_titleTextBase.transform.DOScale(TITLE_TEXT_BASE_TO_SCALE, TITLE_TEXT_BASE_SCALE_ANIMATION_TIME).SetEase(TITLE_TEXT_BASE_SCALE_ANIMATION_EASE));
                var titleTextComponentAnimationSequence = DOTween.Sequence()
                    .SetDelay(TITLE_TEXT_COMPONENT_ANIMATION_DELAY)
                    .AppendCallback(() =>
                    {
                        _titleTextComponent.transform.localScale = TITLE_TEXT_COMPONENT_FROM_SCALE;
                        _titleTextComponent.SetActive(true);
                    })
                    .Append(_titleTextComponent.transform.DOScale(TITLE_TEXT_COMPONENT_TO_SCALE, TITLE_TEXT_COMPONENT_ANIMATION_TIME).SetEase(TITLE_TEXT_COMPONENT_ANIMATION_EASE));
                var titleTextBaseMoveAnimationSequence = DOTween.Sequence()
                    .SetDelay(TITLE_TEXT_BASE_MOVE_ANIMATION_DELAY)
                    .Append(_titleTextBase.transform.DOLocalMove(TITLE_TEXT_BASE_TO_POSITION, TITLE_TEXT_BASE_MOVE_ANIMATION_TIME));
                var contentBaseAnimationSequence = DOTween.Sequence()
                    .SetDelay(CONTENT_BASE_ANIMATION_DELAY)
                    .AppendCallback(() =>
                    {
                        _contentBase.transform.localPosition = CONTENT_BASE_INITIAL_POSITION;
                        _contentBase.SetActive(true);
                    });
                return Observable.WhenAll(
                    titleTextBaseScaleAnimationSequence.OnCompleteAsObservable(),
                    titleTextComponentAnimationSequence.OnCompleteAsObservable(),
                    titleTextBaseMoveAnimationSequence.OnCompleteAsObservable(),
                    contentBaseAnimationSequence.OnCompleteAsObservable()
                ).AsUnitObservable();
            })
            .Delay(TimeSpan.FromSeconds(LAST_DELAY))
            .Do(_ => UIManager.Instance.TryHideTapBlocker());
    }

    public override void Back(DialogInfo info)
    {
    }
    public override void Close(DialogInfo info)
    {
    }
    public override void Open(DialogInfo info)
    {
    }
}
