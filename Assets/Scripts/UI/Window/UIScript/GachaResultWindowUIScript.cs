using DG.Tweening;
using GameBase;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

[ResourcePath("UI/Window/Window-GachaResult")]
public class GachaResultWindowUIScript : WindowBase
{
    [SerializeField] GameObject _labelBase;
    [SerializeField] GameObject _contentBase;
    [SerializeField] GameObject _okButtonBase;
    [SerializeField] CanvasGroup _contentCanvasGroup;
    [SerializeField] List<GameObject> _gachaResultItemBaseList;

    private const float LABEL_DISTANCE = UIManager.FIXED_RES_WIDTH;
    private const float CONTENT_DISTANCE = UIManager.FIXED_RES_WIDTH;
    private const float CONTENT_FROM_FADE = 0.0f;
    private const float CONTENT_TO_FADE = 1.0f;

    private const float CONTENT_FADE_ANIMATION_DELAY = 0.25f;
    private const float CONTENT_FADE_ANIMATION_TIME = 0.25f;
    private const float CONTENT_MOVE_ANIMATION_DELAY = 0.0f;
    private const float CONTENT_MOVE_ANIMATION_TIME = 0.5f;
    private const float LABEL_MOVE_ANIMATION_DELAY = 0.0f;
    private const float LABEL_MOVE_ANIMATION_TIME = 0.5f;
    private const float OPEN_ANIMATION_DELAY = 1.0f;
    private const float OPEN_ANIMATION_INTERVAL = 0.1f;

    private List<ItemMI> itemList;
    private List<GachaResultItem> gachaResultItemList = new List<GachaResultItem>();
    private Vector3 labelInitialPosition;
    private Vector3 contentInitialPosition;

    public override void Init(WindowInfo info)
    {
        // アニメーションタイプをしていするため基底クラスのInit()は呼ばない
        // base.Init(info);

        onClose = (Action)info.param["onClose"];
        _backButton.OnClickIntentAsObservable()
            .Do(_ => UIManager.Instance.CloseWindow(animationType: WindowAnimationType.None))
            .Do(_ => {
                if (onClose != null)
                {
                    onClose();
                    onClose = null;
                }
            })
            .Subscribe();

        itemList = (List<ItemMI>)info.param["itemList"];

        SetInitialize();
        SetReward();
        PlayGachaResultAnimationObservable().Subscribe();
    }

    private void SetInitialize()
    {
        labelInitialPosition = _labelBase.transform.localPosition;
        contentInitialPosition = _contentBase.transform.localPosition;

        _labelBase.transform.DOLocalMoveX(-LABEL_DISTANCE, 0.0f).SetRelative();
        _contentBase.transform.DOLocalMoveX(CONTENT_DISTANCE, 0.0f).SetRelative();

        _contentCanvasGroup.DOFade(CONTENT_FROM_FADE, 0.0f);

        _okButtonBase.SetActive(false);
    }

    private void SetReward()
    {
        itemList.ForEach((item, index) =>
        {
            var parent = _gachaResultItemBaseList[index];
            var gachaResultItem = UIManager.Instance.CreateContent<GachaResultItem>(parent.transform);
            gachaResultItem.InitObservable(item).Subscribe();
            gachaResultItemList.Add(gachaResultItem);
        });
    }

    private IObservable<Unit> PlayGachaResultAnimationObservable()
    {
        return Observable.ReturnUnit()
            .SelectMany(_ =>
            {
                // コンテンツのフェードアニメーション
                var contentFadeAnimationSequence = DOTween.Sequence()
                    .AppendInterval(CONTENT_FADE_ANIMATION_DELAY)
                    .Append(_contentCanvasGroup.DOFade(CONTENT_TO_FADE, CONTENT_FADE_ANIMATION_TIME));

                // コンテンツのムーブアニメーション
                var contentMoveAnimationSequence = DOTween.Sequence()
                    .AppendInterval(CONTENT_MOVE_ANIMATION_DELAY)
                    .Append(_contentBase.transform.DOLocalMove(contentInitialPosition, CONTENT_MOVE_ANIMATION_TIME));

                // ラベルのムーブアニメーション
                var labelMoveAnimationSequence = DOTween.Sequence()
                    .AppendInterval(LABEL_MOVE_ANIMATION_DELAY)
                    .Append(_labelBase.transform.DOLocalMove(labelInitialPosition, LABEL_MOVE_ANIMATION_TIME));

                return Observable.WhenAll(
                    contentFadeAnimationSequence.OnCompleteAsObservable().AsUnitObservable(),
                    contentMoveAnimationSequence.OnCompleteAsObservable().AsUnitObservable(),
                    labelMoveAnimationSequence.OnCompleteAsObservable().AsUnitObservable()
                );
            })
            .Delay(TimeSpan.FromSeconds(OPEN_ANIMATION_DELAY))
            .SelectMany(_ =>
            {
                var observableList = gachaResultItemList.Select(item => item.PlayOpenAnimationObservable()).ToList();
                observableList = observableList.InsertAllBetween(Observable.Timer(TimeSpan.FromSeconds(OPEN_ANIMATION_INTERVAL)).AsUnitObservable());
                return Observable.ReturnUnit().Connect(observableList);
            })
            .Do(_ => _okButtonBase.SetActive(true));
    }

    public override void Open(WindowInfo info)
    {
    }

    public override void Back(WindowInfo info)
    {
    }

    public override void Close(WindowInfo info)
    {
        base.Close(info);
    }
}
