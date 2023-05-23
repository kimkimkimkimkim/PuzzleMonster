using System;
using DG.Tweening;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using PM.Enum.Monster;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using PM.Enum.Battle;

public class VisualFxManager : SingletonMonoBehaviour<VisualFxManager>
{
    // FxItemは基本的に初期状態でSetActiveがfalseにしてある

    #region FxItem

    /// <summary>
    /// クエストタイトル表示演出を実行
    /// </summary>
    public IObservable<Unit> PlayQuestTitleFxObservable(string title)
    {
        return PMAddressableAssetUtil.InstantiateVisualFxItemObservable<QuestTitleFx>(FadeManager.Instance.GetFadeCanvasRT())
            .SelectMany(fx =>
            {
                fx.text.SetAlpha(0);
                fx.text.text = title;
                fx.gameObject.SetActive(true);

                return DOTween.Sequence()
                    .Append(DOVirtual.Float(0.0f, 1.0f, 1.0f, value => fx.text.SetAlpha(value)))
                    .AppendInterval(2.0f)
                    .Append(DOVirtual.Float(1.0f, 0.0f, 1.0f, value => fx.text.SetAlpha(value)))
                    .OnCompleteAsObservable()
                    .Do(_ =>
                    {
                        if (fx.gameObject != null) Addressables.ReleaseInstance(fx.gameObject);
                    })
                    .AsUnitObservable();
            });
    }

    /// <summary>
    /// ウェーブ表示演出を実行
    /// </summary>
    public IObservable<Unit> PlayWaveTitleFxObservable(Transform parent, int currentWaveCount, int maxWaveCount)
    {
        return PMAddressableAssetUtil.InstantiateVisualFxItemObservable<WaveTitleFx>(parent)
            .SelectMany(fx =>
            {
                var distance = 100.0f;

                fx.text.SetAlpha(0);
                fx.text.text = $"Wave {currentWaveCount}/{maxWaveCount}";
                fx.gameObject.SetActive(true);

                return DOTween.Sequence()
                    .Append(fx.transform.DOLocalMoveX(distance, 0.0f))
                    .Append(DOVirtual.Float(0.0f, 1.0f, 1.0f, value => fx.text.SetAlpha(value)))
                    .Join(fx.transform.DOLocalMoveX(0.0f, 1.0f))
                    .AppendInterval(1.0f)
                    .Append(DOVirtual.Float(1.0f, 0.0f, 1.0f, value => fx.text.SetAlpha(value)))
                    .Join(fx.transform.DOLocalMoveX(-distance, 1.0f))
                    .OnCompleteAsObservable()
                    .Do(_ =>
                    {
                        if (fx.gameObject != null) Addressables.ReleaseInstance(fx.gameObject);
                    })
                    .AsUnitObservable();
            });
    }

    /// <summary>
    /// 攻撃開始アニメーションの再生
    /// 攻撃するモンスターが前に出て戻るアニメーション
    /// </summary>
    public IObservable<Unit> PlayStartAttackFxObservable(RectTransform doMonsterRT, bool isPlayer)
    {
        const float SCALE_ANIMATION_TIME = 0.1f;
        const float GO_ANIMATION_TIME = 0.25f;
        const float BACK_ANIMATION_TIME = 0.25f;
        const float MOVE_X_DISTANCE = 20.0f;
        const float MOVE_Y_DISTANCE = 10.0f;

        var defaultPosition = doMonsterRT.localPosition;
        var defaultPivot = doMonsterRT.pivot;

        return Observable.ReturnUnit()
            .SelectMany(_ =>
            {
                return DOTween.Sequence()
                    .Append(doMonsterRT.DOLocalMoveX(isPlayer ? defaultPosition.x + MOVE_X_DISTANCE : defaultPosition.x - MOVE_X_DISTANCE, GO_ANIMATION_TIME))
                    .Join(doMonsterRT.DOLocalMoveY(defaultPosition.y + MOVE_Y_DISTANCE, GO_ANIMATION_TIME / 2).SetEase(Ease.OutSine))
                    .Join(doMonsterRT.DOLocalMoveY(defaultPosition.y, GO_ANIMATION_TIME / 2).SetEase(Ease.OutSine).SetDelay(GO_ANIMATION_TIME / 2))
                    .OnCompleteAsObservable()
                    .AsUnitObservable();
            })
            .Do(_ =>
            {
                // もとに戻るアニメーションは別のストリームで行う
                doMonsterRT.DOLocalMoveX(defaultPosition.x, BACK_ANIMATION_TIME);
            })
            .AsUnitObservable();
    }

    /// <summary>
    /// 攻撃不能アニメーション
    /// ぶるぶるさせてミステキストを表示
    /// </summary>
    public IObservable<Unit> PlayActionFailedAnimationObservable(BattleMonsterItem doBattleMonsterItem)
    {
        const int SHAKE_TIME = 3;
        const float SHAKE_DISTANCE = 10.0f;
        const float ONE_SHAKE_ANIMATION_TIME = 0.1f;
        const float MISS_TEXT_DISTANCE = 34.0f;
        const float MISS_TEXT_ANIMATION_TIME = 0.7f;
        const float MISS_TEXT_WAIT_TIME = 0.3f;
        const Ease MISS_TEXT_ANIMATION_EASE = Ease.OutQuint;

        var shakeAnimationSequence = DOTween.Sequence().Append(doBattleMonsterItem.monsterImageBase.transform.DOLocalMoveX(SHAKE_DISTANCE / 2, ONE_SHAKE_ANIMATION_TIME / 2));
        for (var i = 0; i < SHAKE_TIME; i++)
        {
            shakeAnimationSequence
                .Append(doBattleMonsterItem.monsterImageBase.transform.DOLocalMoveX(-SHAKE_DISTANCE, ONE_SHAKE_ANIMATION_TIME))
                .Append(doBattleMonsterItem.monsterImageBase.transform.DOLocalMoveX(SHAKE_DISTANCE, ONE_SHAKE_ANIMATION_TIME));
        }
        shakeAnimationSequence
            .Append(doBattleMonsterItem.monsterImageBase.transform.DOLocalMoveX(-SHAKE_DISTANCE, ONE_SHAKE_ANIMATION_TIME))
            .Append(doBattleMonsterItem.monsterImageBase.transform.DOLocalMoveX(SHAKE_DISTANCE / 2, ONE_SHAKE_ANIMATION_TIME / 2));

        var missTextAnimationSequence = DOTween.Sequence()
            .AppendCallback(() => doBattleMonsterItem.missText.gameObject.SetActive(true))
            .Append(doBattleMonsterItem.missText.transform.DOLocalMoveY(MISS_TEXT_DISTANCE, MISS_TEXT_ANIMATION_TIME).SetEase(MISS_TEXT_ANIMATION_EASE))
            .SetDelay(MISS_TEXT_WAIT_TIME)
            .AppendCallback(() => doBattleMonsterItem.missText.gameObject.SetActive(false))
            .Append(doBattleMonsterItem.missText.transform.DOLocalMoveY(-MISS_TEXT_DISTANCE, 0));

        return Observable.WhenAll(
            shakeAnimationSequence.OnCompleteAsObservable().AsUnitObservable(),
            missTextAnimationSequence.OnCompleteAsObservable().AsUnitObservable()
        );
    }

    /// <summary>
    /// バトル勝利演出を実行
    /// </summary>
    public IObservable<Unit> PlayWinBattleFxObservable(Transform parent)
    {
        var animationTime = 0.5f;
        var delayTime = 0.1f;
        var moveXEase = Ease.InSine;
        var moveYEase = Ease.OutSine;
        var fadeEase = Ease.InQuad;

        return PMAddressableAssetUtil.InstantiateVisualFxItemObservable<WinBattleFx>(parent)
            .SelectMany(fx =>
            {
                var initialPosition = fx.textInitialPositionTransform.position;
                var toPositionList = new List<Vector3>();
                fx.textList.ForEach(text =>
                {
                    text.SetAlpha(0);
                    toPositionList.Add(text.transform.position);
                    text.transform.position = initialPosition;
                });
                fx.gameObject.SetActive(true);

                var observableList = fx.textList.Select((text, index) =>
                {
                    var toPosition = toPositionList[index];

                    return Observable.Timer(TimeSpan.FromSeconds(delayTime * index))
                        .SelectMany(_ =>
                        {
                            return DOTween.Sequence()
                                .Append(text.transform.DOMoveX(toPosition.x, animationTime).SetEase(moveXEase))
                                .Join(text.transform.DOMoveY(toPosition.y, animationTime).SetEase(moveYEase))
                                .Join(DOVirtual.Float(0.0f, 1.0f, animationTime, value => text.SetAlpha(value)).SetEase(fadeEase))
                                .OnCompleteAsObservable()
                                .AsUnitObservable();
                        });
                });

                return Observable.WhenAll(observableList);
            });
    }

    public IObservable<Unit> PlayLoseBattleFxObservable(Transform parent)
    {
        return PMAddressableAssetUtil.InstantiateVisualFxItemObservable<LoseBattleFx>(parent)
            .SelectMany(fx =>
            {
                return Observable.Timer(TimeSpan.FromSeconds(1))
                    .Do(_ =>
                    {
                        if (fx.gameObject != null) Addressables.ReleaseInstance(fx.gameObject);
                    })
                    .AsUnitObservable();
            });
    }

    public IObservable<Unit> PlaySkillFxObservable(SkillFxMB skillFx, Transform fxParent, Image fxBackgroundImage = null)
    {
        SkillFxItem skillFxItem = null;
        Sprite skillFxSprite = null;

        return Observable.WhenAll(
            PMAddressableAssetUtil.InstantiateSkillFxItemObservable(fxParent, skillFx.id).Do(item => skillFxItem = item).AsUnitObservable(),
            PMAddressableAssetUtil.GetSkillFxSpriteObservable(skillFx.id).Do(sprite => skillFxSprite = sprite).AsUnitObservable()
        )
            .SelectMany(unit =>
            {
                const float SKILL_FX_SIZE = 1.3f;
                const float ALPHA = 0.2f;
                const float FADE_TIME = 0.2f;

                // テクスチャの設定
                skillFxItem.renderer.material.mainTexture = skillFxSprite.texture;

                // タイルの設定
                var textureSheetAnimation = skillFxItem.particleSystem.textureSheetAnimation;
                textureSheetAnimation.numTilesX = skillFx.numTilesX;
                textureSheetAnimation.numTilesY = skillFx.numTilesY;

                // アニメーション時間の設定
                var numTiles = skillFx.numTilesX * skillFx.numTilesY;
                var animationTime = (float)numTiles / 10;
                var main = skillFxItem.particleSystem.main;
                // main.duration = animationTime;
                main.startLifetime = animationTime;

                // サイズの設定
                var spriteWidth = skillFxSprite.rect.width / skillFx.numTilesX;
                var spriteHeight = skillFxSprite.rect.height / skillFx.numTilesY;
                var scaleX = SKILL_FX_SIZE * skillFx.sizeScale;
                var scaleY = (spriteHeight / spriteWidth) * SKILL_FX_SIZE * skillFx.sizeScale;
                skillFxItem.gameObject.transform.localScale = new Vector3(scaleX, scaleY, scaleX);

                // 座標の設定
                skillFxItem.gameObject.transform.localPosition = new Vector3(skillFx.offsetX, skillFx.offsetY, 0);

                // 演出再生
                var skillFxObservable = Observable.Timer(TimeSpan.FromSeconds(FADE_TIME)).Do(_ => skillFxItem.particleSystem.PlayWithRelease(animationTime)).Delay(TimeSpan.FromSeconds(animationTime)).AsUnitObservable();

                // 演出背景の表示
                var backgroundSequence = fxBackgroundImage == null ?
                    DOTween.Sequence() :
                    DOTween.Sequence()
                        .Append(fxBackgroundImage.DOFade(ALPHA, FADE_TIME))
                        .AppendInterval(animationTime + 0.1f)
                        .Append(fxBackgroundImage.DOFade(0.0f, FADE_TIME));

                return Observable.WhenAll(
                    skillFxObservable,
                    backgroundSequence.OnCompleteAsObservable().AsUnitObservable()
                );
            })
            .AsUnitObservable();
    }

    /// <summary>
    /// 被ダメージ演出の再生
    /// </summary>
    public IObservable<Unit> PlayTakeDamageFxObservable(BeDoneBattleMonsterData beDoneBattleMonsterData, BattleMonsterItem battleMonsterItem, long skillFxId, int toHp, int toEnergy, int toShield, Transform fxParent, Image fxBackgroundImage)
    {
        const float SLIDER_ANIMATION_TIME = 1.5f;
        const float DAMAGE_FX_BIG_SCALE = 1.5f;
        const float DAMAGE_FX_SMALL_SCALE = 1.2f;
        const float DAMAGE_FX_SCALE_ANIMATION_TIME = 0.4f;
        const float DAMAGE_FX_MOVE_ANIMATION_TIME = 0.8f;
        const float DAMAGE_FX_MOVE_ANIMATION_OFFSET_Y = 75.0f;
        const float DAMAGE_FX_MOVE_ANIMATION_DELAY_TIME = 0.1f;
        const float DAMAGE_FX_FADE_DELAY_TIME = 0.2f;

        var skillFx = MasterRecord.GetMasterOf<SkillFxMB>().Get(skillFxId);
        DamageFx damageFX = null;

        return PMAddressableAssetUtil.InstantiateVisualFxItemObservable<DamageFx>(battleMonsterItem.effectBase).Do(fx => damageFX = fx).AsUnitObservable()
            .SelectMany(_ =>
            {
                if (skillFx != null)
                {
                    return PlaySkillFxObservable(skillFx, fxParent, fxBackgroundImage);
                }
                else
                {
                    return Observable.ReturnUnit();
                }
            })
            .SelectMany(_ =>
            {
                var damageAnimationObservable = Observable.ReturnUnit()
                    .SelectMany(_ =>
                    {
                        damageFX.SetText(beDoneBattleMonsterData);
                        damageFX.gameObject.SetActive(true);

                        // 拡大アニメーションが終わったら次の処理にいく
                        var scaleSequence = DOTween.Sequence()
                            .Append(damageFX.transform.DOScale(DAMAGE_FX_BIG_SCALE, DAMAGE_FX_SCALE_ANIMATION_TIME / 4))
                            .Append(damageFX.transform.DOScale(1.0f, DAMAGE_FX_SCALE_ANIMATION_TIME / 4))
                            .Append(damageFX.transform.DOScale(DAMAGE_FX_SMALL_SCALE, DAMAGE_FX_SCALE_ANIMATION_TIME / 4))
                            .Append(damageFX.transform.DOScale(1.0f, DAMAGE_FX_SCALE_ANIMATION_TIME / 4));
                        return scaleSequence.OnCompleteAsObservable()
                            .Do(tween =>
                            {
                                // ムーブアニメーションは個別で実行
                                var moveSequence = DOTween.Sequence()
                                    .AppendInterval(DAMAGE_FX_MOVE_ANIMATION_DELAY_TIME)
                                    .Append(damageFX.transform.DOLocalMoveY(DAMAGE_FX_MOVE_ANIMATION_OFFSET_Y, DAMAGE_FX_MOVE_ANIMATION_TIME))
                                    .Join(damageFX.canvasGroup.DOFade(0.0f, DAMAGE_FX_MOVE_ANIMATION_TIME).SetDelay(DAMAGE_FX_FADE_DELAY_TIME));
                                moveSequence.OnCompleteAsObservable().Do(t => { if (damageFX.gameObject != null) Addressables.ReleaseInstance(damageFX.gameObject); }).Subscribe();
                            });
                    })
                    .AsUnitObservable();

                var hpSliderAnimationSequence = DOTween.Sequence()
                    .Append(DOVirtual.Float(battleMonsterItem.hpSlider.value, toHp, SLIDER_ANIMATION_TIME, value => battleMonsterItem.hpSlider.value = value));
                var shieldSliderAnimationSequence = DOTween.Sequence()
                    .Append(DOVirtual.Float(battleMonsterItem.shieldSlider.value, toShield, SLIDER_ANIMATION_TIME, value => battleMonsterItem.shieldSlider.value = value))
                    .AppendCallback(() => battleMonsterItem.ShowShieldSlider(toShield > 0));

                return Observable.WhenAll(
                    damageAnimationObservable,
                    hpSliderAnimationSequence.OnCompleteAsObservable().AsUnitObservable(),
                    shieldSliderAnimationSequence.OnCompleteAsObservable().AsUnitObservable(),
                    PlayEnergySliderAnimationObservable(battleMonsterItem.energySlider, toEnergy)
                );
            });
    }

    public IObservable<Unit> PlayEnergySliderAnimationObservable(Slider slider, int toEnergy)
    {
        const float SLIDER_ANIMATION_TIME = 0.0f;

        return Observable.ReturnUnit()
            .SelectMany(_ =>
            {
                return DOTween.Sequence()
                    .Append(DOVirtual.Float(slider.value, toEnergy, SLIDER_ANIMATION_TIME, value => slider.value = value))
                    .OnCompleteAsObservable();
            })
            .AsUnitObservable();
    }

    public IObservable<Unit> PlayDieAnimationObservable(BattleMonsterItem battleMonsterItem)
    {
        const long DIE_ANIMATION_SKILL_FXID = 64;
        const float DELAY_TIME = 0.5f;

        var skillFx = MasterRecord.GetMasterOf<SkillFxMB>().Get(DIE_ANIMATION_SKILL_FXID);
        return Observable.ReturnUnit()
            .Do(_ => battleMonsterItem.ShowMonsterImage(false))
            .SelectMany(_ => PlaySkillFxObservable(skillFx, battleMonsterItem.effectBase))
            .Do(_ =>
            {
                battleMonsterItem.hpSlider.value = 0;
                battleMonsterItem.energySlider.value = 0;
                battleMonsterItem.shieldSlider.value = 0;
                battleMonsterItem.RefreshBattleCondition(new List<BattleConditionInfo>());
                battleMonsterItem.ShowGraveImage(true);
            })
            .Delay(TimeSpan.FromSeconds(DELAY_TIME));
    }

    #endregion FxItem
}