using DG.Tweening;
using GameBase;
using PM.Enum.Item;
using PM.Enum.Monster;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-GachaResultItem")]
public class GachaResultItem : MonoBehaviour
{
    [SerializeField] private Image _chestImage;
    [SerializeField] private GameObject _chestBase;
    [SerializeField] private IconItem _iconItem;
    [SerializeField] private Sprite _rSprite;
    [SerializeField] private Sprite _srSprite;
    [SerializeField] private Sprite _ssrSprite;
    [SerializeField] private ParticleSystem _rayPS;

    private Vector3 CHEST_INITIAL_SCALE = Vector3.one;
    private Vector3 CHEST_DOWN_SCALE = Vector3.one * 0.8f;
    private Vector3 CHEST_UP_SCALE = Vector3.one * 0.9f;
    private const float CHEST_FROM_ALPHA = 1.0f;
    private const float CHEST_TO_ALPHA = 0.0f;
    private Vector3 RAY_INITIAL_SCALE = Vector3.zero;
    private Vector3 RAY_UP_SCALE = Vector3.one * 1.4f;
    private Vector3 RAY_DOWN_SCALE = Vector3.zero;
    private Vector3 RAY_SSR_SCALE = Vector3.one;

    // 時間関係
    private const float CHEST_SCALE_DOWN_ANIMATION_DELAY = 0.0f;

    private const float CHEST_SCALE_DOWN_ANIMATION_TIME = 0.2f;
    private const float CHEST_SCALE_UP_ANIMATION_DELAY = 0.0f;
    private const float CHEST_SCALE_UP_ANIMATION_TIME = 0.1f;
    private const float CHEST_FADE_OUT_ANIMATION_DELAY = CHEST_SCALE_DOWN_ANIMATION_DELAY + CHEST_SCALE_DOWN_ANIMATION_TIME + CHEST_SCALE_UP_ANIMATION_DELAY;
    private const float CHEST_FADE_OUT_ANIMATION_TIME = CHEST_SCALE_UP_ANIMATION_TIME;
    private const float RAY_SCALE_UP_ANIMATION_DELAY = CHEST_FADE_OUT_ANIMATION_DELAY - 0.02f;
    private const float RAY_SCALE_UP_ANIMATION_TIME = 0.2f;
    private const float RAY_SCALE_DOWN_ANIMATION_DELAY = 0.1f;
    private const float RAY_SCALE_DOWN_ANIMATION_TIME = 0.2f;
    private const float ICON_ANIMATION_DELAY = CHEST_FADE_OUT_ANIMATION_DELAY + CHEST_FADE_OUT_ANIMATION_TIME;

    private MonsterMB monster;

    public IObservable<Unit> InitObservable(ItemMI itemMI)
    {
        // モンスター以外は今のところ受け付けていない
        if (itemMI.itemType != ItemType.Monster) return Observable.ReturnUnit();

        monster = MasterRecord.GetMasterOf<MonsterMB>().Get(itemMI.itemId);
        return Observable.ReturnUnit()
            .Do(_ =>
            {
                // 宝箱は表示、アイコンは非表示
                _chestBase.SetActive(true);
                _iconItem.gameObject.SetActive(false);
                _iconItem.ShowRarityImage(false);

                // 画像の設定
                _chestImage.sprite = GetChestSprite(monster);
                _iconItem.SetIcon(itemMI);

                // その他の調整
                _chestBase.transform.localScale = CHEST_INITIAL_SCALE;
                _chestImage.DOFade(CHEST_FROM_ALPHA, 0.0f);
                _rayPS.transform.localScale = RAY_INITIAL_SCALE;
            });
    }

    public IObservable<Unit> PlayOpenAnimationObservable()
    {
        return Observable.ReturnUnit()
            .SelectMany(_ =>
            {
                // 宝箱のスケールアニメーション
                var chestScaleAnimationSequence = DOTween.Sequence()
                    .AppendInterval(CHEST_SCALE_DOWN_ANIMATION_DELAY)
                    .Append(_chestBase.transform.DOScale(CHEST_DOWN_SCALE, CHEST_SCALE_DOWN_ANIMATION_TIME))
                    .AppendInterval(CHEST_SCALE_UP_ANIMATION_DELAY)
                    .Append(_chestBase.transform.DOScale(CHEST_UP_SCALE, CHEST_SCALE_UP_ANIMATION_TIME));

                // 宝箱のフェードアニメーション
                var chestFadeAnimationSequence = DOTween.Sequence()
                    .AppendInterval(CHEST_FADE_OUT_ANIMATION_DELAY)
                    .Append(_chestImage.DOFade(CHEST_TO_ALPHA, CHEST_FADE_OUT_ANIMATION_TIME));

                // 後光のスケールアニメーション
                var rayToScale = monster.rarity == MonsterRarity.SSR ? RAY_SSR_SCALE : RAY_DOWN_SCALE;
                var rayScaleAnimationSequence = DOTween.Sequence()
                    .AppendInterval(RAY_SCALE_UP_ANIMATION_DELAY)
                    .AppendCallback(() => _rayPS.Play())
                    .Append(_rayPS.transform.DOScale(RAY_UP_SCALE, RAY_SCALE_UP_ANIMATION_TIME))
                    .AppendInterval(RAY_SCALE_DOWN_ANIMATION_DELAY)
                    .Append(_rayPS.transform.DOScale(rayToScale, RAY_SCALE_DOWN_ANIMATION_TIME));

                // アイコンのアニメーション
                var iconAnimationSequence = DOTween.Sequence()
                    .AppendInterval(ICON_ANIMATION_DELAY)
                    .AppendCallback(() =>
                    {
                        _iconItem.gameObject.SetActive(true);
                        _iconItem.ShowRarityImage(true);
                    });

                return Observable.WhenAll(
                    chestScaleAnimationSequence.OnCompleteAsObservable().AsUnitObservable(),
                    chestFadeAnimationSequence.OnCompleteAsObservable().AsUnitObservable(),
                    rayScaleAnimationSequence.OnCompleteAsObservable().AsUnitObservable(),
                    iconAnimationSequence.OnCompleteAsObservable().AsUnitObservable()
                );
            });
    }

    private Sprite GetChestSprite(MonsterMB monster)
    {
        switch (monster.rarity)
        {
            case MonsterRarity.R:
                return _rSprite;

            case MonsterRarity.SR:
                return _srSprite;

            case MonsterRarity.SSR:
                return _ssrSprite;

            case MonsterRarity.N:
            default:
                return _rSprite;
        }
    }
}