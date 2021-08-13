using System;
using DG.Tweening;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using PM.Enum.Monster;

public class VisualFxManager : SingletonMonoBehaviour<VisualFxManager>
{
    public IObservable<Unit> PlayQuestTitleFxObservable(string title)
    {
        return PMAddressableAssetUtil.InstantiateVisualFxItemObservable<QuestTitleFx>(FadeManager.Instance.GetFadeCanvasRT())
            .SelectMany(fx => {
                fx.text.SetAlpha(0);
                fx.text.text = title;
                fx.gameObject.SetActive(true);

                return DOTween.Sequence()
                    .Append(DOVirtual.Float(0.0f, 1.0f, 1.0f, value => fx.text.SetAlpha(value)))
                    .AppendInterval(2.0f)
                    .Append(DOVirtual.Float(1.0f, 0.0f, 1.0f, value => fx.text.SetAlpha(value)))
                    .OnCompleteAsObservable()
                    .Do(_ => {
                        if(fx.gameObject != null) Addressables.ReleaseInstance(fx.gameObject);
                    })
                    .AsUnitObservable();
            });
    }

    public IObservable<Unit> PlayWaveTitleFxObservable(Transform parent, int currentWaveCount, int maxWaveCount)
    {
        return PMAddressableAssetUtil.InstantiateVisualFxItemObservable<WaveTitleFx>(parent)
            .SelectMany(fx => {
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
                    .Do(_ => {
                        if(fx.gameObject != null) Addressables.ReleaseInstance(fx.gameObject);
                    })
                    .AsUnitObservable();
            });
    }

    public IObservable<Unit> PlayEnemyAttackFxObservable(Transform parent, QuestMonsterItem item, Transform backgroundImageTransform)
    {
        var animationTime = 0.25f;
        var scaleEndValue = 1.4f;
        var hopDistanceY = 20.0f;
        var downMagnification = 2f;

        return DOTween.Sequence()
            .Append(DOTween.Sequence().Append(item.transform.DOLocalMoveY(hopDistanceY, animationTime / 2).SetRelative().SetEase(Ease.OutSine)).Append(item.transform.DOLocalMoveY(-hopDistanceY * downMagnification, animationTime / 2).SetRelative().SetEase(Ease.InSine)))
            .Join(item.transform.DOScaleX(scaleEndValue, animationTime))
            .Join(item.transform.DOScaleY(scaleEndValue, animationTime))
            .Append(backgroundImageTransform.DOShakePosition(0.25f, new Vector3(50,50,0)))
            .Append(DOTween.Sequence().Append(item.transform.DOLocalMoveY(hopDistanceY * downMagnification, animationTime / 2).SetRelative().SetEase(Ease.OutSine)).Append(item.transform.DOLocalMoveY(-hopDistanceY, animationTime / 2).SetRelative().SetEase(Ease.InSine)))
            .Join(item.transform.DOScaleX(1, animationTime))
            .Join(item.transform.DOScaleY(1, animationTime))
            .OnCompleteAsObservable()
            .AsUnitObservable();
    }

    public IObservable<Unit> PlayPlayerAttackFxObservable(Transform parent, Vector3 fromPosition, Vector3 toPosition, MonsterAttribute attribute, long attackId)
    {
        ParticleSystem orbitPS = null;
        ParticleSystem fxPS = null;
        return Observable.WhenAll(
            PMAddressableAssetUtil.InstantiateNormalAttackOrbitObservable(parent, attribute).Do(ps => orbitPS = ps).AsUnitObservable(),
            PMAddressableAssetUtil.InstantiateNormalAttackFxObservable(parent, attackId).Do(ps => fxPS = ps).AsUnitObservable()
        )
            .SelectMany(_ =>
            {
                orbitPS.transform.position = new Vector3(fromPosition.x, fromPosition.y, 0);
                fxPS.transform.position = new Vector3(toPosition.x, toPosition.y, 0);

                return DOTween.Sequence()
                    .AppendCallback(() => orbitPS.Play())
                    .Append(orbitPS.transform.DOMoveX(toPosition.x, 0.5f).SetEase(Ease.Linear))
                    .Join(orbitPS.transform.DOMoveY(toPosition.y, 0.5f).SetEase(Ease.OutQuad))
                    .AppendCallback(() =>
                    {
                        if (orbitPS.gameObject != null) Addressables.ReleaseInstance(orbitPS.gameObject);
                        fxPS.Play();
                    })
                    .AppendInterval(1.0f)
                    .AppendCallback(() =>
                    {
                        if (fxPS.gameObject != null) Addressables.ReleaseInstance(fxPS.gameObject);
                    })
                    .OnCompleteAsObservable()
                    .AsUnitObservable();
            });
    }

    public IObservable<QuestMonsterItem> PlayCreateMonsterFxObservable(Transform parent, long monsterId)
    {
        // 最初に一度に生成する必要があるため、生成処理はObservableの外に出す
        var item = UIManager.Instance.CreateContent<QuestMonsterItem>(parent);
        var canvasGroup = item.GetCanvasGroup();
        canvasGroup.alpha = 0;

        return item.SetMonsterImageObservable(monsterId)
            .SelectMany(_ =>
            {
                return DOTween.Sequence()
                    .Append(canvasGroup.DOFade(1.0f, 0.5f))
                    .OnCompleteAsObservable()
                    .AsUnitObservable();
            })
            .Select(_ => item);
    }

    public IObservable<Unit> PlayDefeatMonsterFxObservable(QuestMonsterItem item)
    {
        var canvasGroup = item.GetCanvasGroup();
        return DOTween.Sequence()
            .Append(canvasGroup.DOFade(0.0f, 0.5f))
            .OnCompleteAsObservable()
            .AsUnitObservable();
    }
}
