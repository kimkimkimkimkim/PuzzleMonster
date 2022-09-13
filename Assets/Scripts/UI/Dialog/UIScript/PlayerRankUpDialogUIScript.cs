using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using DG.Tweening;

[ResourcePath("UI/Dialog/Dialog-PlayerRankUp")]
public class PlayerRankUpDialogUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected Text _mainRankText;
    [SerializeField] protected Text _beforeRankText;
    [SerializeField] protected Text _afterRankText;
    [SerializeField] protected Text _beforeMaxStaminaText;
    [SerializeField] protected Text _afterMaxStaminaText;
    [SerializeField] protected Text _riseStaminaText;
    [SerializeField] protected GameObject _titleTextBase;
    [SerializeField] protected GameObject _rankIconBase;
    [SerializeField] protected GameObject _rankTextBase;
    [SerializeField] protected GameObject _maxStaminaTextBase;
    [SerializeField] protected GameObject _riseStaminaTextBase;

    private void Awake()
    {
        _titleTextBase.SetActive(false);
        _rankIconBase.SetActive(false);
        _rankTextBase.SetActive(false);
        _maxStaminaTextBase.SetActive(false);
        _riseStaminaTextBase.SetActive(false);
    }

    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action)info.param["onClickClose"];
        var beforeRank = (int)info.param["beforeRank"];
        var afterRank = (int)info.param["afterRank"];
        var beforeMaxStamina = (int)info.param["beforeMaxStamina"];
        var afterMaxStamina = (int)info.param["afterMaxStamina"];

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

        _mainRankText.text = afterRank.ToString();
        _beforeRankText.text = beforeRank.ToString();
        _afterRankText.text = afterRank.ToString();
        _beforeMaxStaminaText.text = beforeMaxStamina.ToString();
        _afterMaxStaminaText.text = afterMaxStamina.ToString();
        SetRiseStaminaText(beforeRank, afterRank);
        PlayAnimationObservable().Subscribe();
    }

    private void SetRiseStaminaText(int beforeRank, int afterRank)
    {
        var staminaList = MasterRecord.GetMasterOf<StaminaMB>().GetAll().ToList();
        var riseStamina = 0;
        for (var rank = afterRank; rank > beforeRank; rank--)
        {
            var stamina = staminaList.First(m => m.rank == rank);
            riseStamina += stamina.stamina;
        }
        _riseStaminaText.text = $"プレイヤーランクが上がり、スタミナが<color=#F2548D>{riseStamina}</color>しました";
    }

    private IObservable<Unit> PlayAnimationObservable()
    {
        var TITLE_TEXT_FROM_SCALE = new Vector3(1.0f, 0.0f, 1.0f);
        var TITLE_TEXT_TO_SCALE = new Vector3(1.0f, 1.0f, 1.0f);
        var RANK_ICON_FROM_SCALE = Vector3.one * 0.6f;
        var RANK_ICON_MAX_SCALE = Vector3.one * 1.15f;
        var RANK_ICON_TO_SCALE = Vector3.one * 1.0f;
        var RANK_TEXT_INITIAL_POSITION = _rankTextBase.transform.localPosition;
        var RANK_TEXT_OFFSET = Vector3.left * 500.0f;
        var MAX_STAMINA_TEXT_INITIAL_POSITION = _maxStaminaTextBase.transform.localPosition;
        var MAX_STAMINA_TEXT_OFFSET = Vector3.left * 500.0f;

        const float TITLE_TEXT_ANIMATION_TIME = 0.4f;
        const float RANK_ICON_SCALE_UP_ANIMATION_TIME = 0.2f;
        const float RANK_ICON_SCALE_DOWN_ANIMATION_TIME = 0.2f;
        const float RANK_TEXT_ANIMATION_TIME = 0.5f;
        const float MAX_STAMINA_TEXT_ANIMATION_TIME = 0.5f;

        const float TITLE_TEXT_ANIMATION_DELAY = 0.0f;
        const float RANK_ICON_ANIMATION_DELAY = TITLE_TEXT_ANIMATION_DELAY + TITLE_TEXT_ANIMATION_TIME + 0.3f;
        const float RANK_TEXT_ANIMATION_DELAY = RANK_ICON_ANIMATION_DELAY + RANK_ICON_SCALE_UP_ANIMATION_TIME + RANK_ICON_SCALE_DOWN_ANIMATION_TIME + 0.5f;
        const float MAX_STAMINA_TEXT_ANIMATION_DELAY = RANK_TEXT_ANIMATION_DELAY + 0.8f;
        const float RISE_STAMINA_TEXT_ANIMATION_DELAY = MAX_STAMINA_TEXT_ANIMATION_DELAY + MAX_STAMINA_TEXT_ANIMATION_TIME + 0.4f;

        return Observable.ReturnUnit()
            .Do(_ => UIManager.Instance.ShowTapBlocker())
            .SelectMany(_ =>
            {
                var titleTextAnimationSequence = DOTween.Sequence()
                    .SetDelay(TITLE_TEXT_ANIMATION_DELAY)
                    .AppendCallback(() =>
                    {
                        _titleTextBase.transform.localScale = TITLE_TEXT_FROM_SCALE;
                        _titleTextBase.SetActive(true);
                    })
                    .Append(_titleTextBase.transform.DOScale(TITLE_TEXT_TO_SCALE, TITLE_TEXT_ANIMATION_TIME));
                var rankIconAnimationSequence = DOTween.Sequence()
                    .SetDelay(RANK_ICON_ANIMATION_DELAY)
                    .AppendCallback(() =>
                    {
                        _rankIconBase.transform.localScale = RANK_ICON_FROM_SCALE;
                        _rankIconBase.SetActive(true);
                    })
                    .Append(_rankIconBase.transform.DOScale(RANK_ICON_MAX_SCALE, RANK_ICON_SCALE_UP_ANIMATION_TIME))
                    .Append(_rankIconBase.transform.DOScale(RANK_ICON_TO_SCALE, RANK_ICON_SCALE_DOWN_ANIMATION_TIME));
                var rankTextAnimationSequence = DOTween.Sequence()
                    .SetDelay(RANK_TEXT_ANIMATION_DELAY)
                    .AppendCallback(() =>
                    {
                        _rankTextBase.transform.localPosition = RANK_TEXT_INITIAL_POSITION + RANK_TEXT_OFFSET;
                        _rankTextBase.SetActive(true);
                    })
                    .Append(_rankTextBase.transform.DOLocalMove(RANK_TEXT_INITIAL_POSITION, RANK_TEXT_ANIMATION_TIME));
                var maxStaminaAnimationSequence = DOTween.Sequence()
                    .SetDelay(MAX_STAMINA_TEXT_ANIMATION_DELAY)
                    .AppendCallback(() =>
                    {
                        _maxStaminaTextBase.transform.localPosition = MAX_STAMINA_TEXT_INITIAL_POSITION + MAX_STAMINA_TEXT_OFFSET;
                        _maxStaminaTextBase.SetActive(true);
                    })
                    .Append(_maxStaminaTextBase.transform.DOLocalMove(MAX_STAMINA_TEXT_INITIAL_POSITION, MAX_STAMINA_TEXT_ANIMATION_TIME));
                var riseStaminaTextAnimationSequence = DOTween.Sequence()
                    .SetDelay(RISE_STAMINA_TEXT_ANIMATION_DELAY)
                    .AppendCallback(() =>
                    {
                        _riseStaminaTextBase.SetActive(true);
                    });

                return Observable.WhenAll(
                    titleTextAnimationSequence.OnCompleteAsObservable(),
                    rankIconAnimationSequence.OnCompleteAsObservable(),
                    rankTextAnimationSequence.OnCompleteAsObservable(),
                    maxStaminaAnimationSequence.OnCompleteAsObservable(),
                    riseStaminaTextAnimationSequence.OnCompleteAsObservable()
                ).AsUnitObservable();
            })
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
