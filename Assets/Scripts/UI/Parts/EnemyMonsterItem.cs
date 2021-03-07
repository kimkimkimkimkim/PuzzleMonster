using System;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;

public class EnemyMonsterItem : MonoBehaviour
{
    [SerializeField] protected Image _hpGaugeImage;

    private int maxHp;
    private Sequence hpGaugeAnimationSequence;

    public void Init(int maxHp)
    {
        this.maxHp = maxHp;
        _hpGaugeImage.fillAmount = 1;
    }

    public IObservable<Unit> PlayHpGaugeAnimationObservable(int hp)
    {
        const float ANIMATION_TIME = 0.5f;

        var endGaugeValue = (float)hp / maxHp;
        var currentGaugeValue = _hpGaugeImage.fillAmount;
        if (hpGaugeAnimationSequence != null && hpGaugeAnimationSequence.IsActive() && hpGaugeAnimationSequence.IsPlaying())
        {
            hpGaugeAnimationSequence.Kill();
        }
        hpGaugeAnimationSequence = DOTween.Sequence()
            .Append(_hpGaugeImage.DOFillAmount(endGaugeValue, ANIMATION_TIME));

        return hpGaugeAnimationSequence.PlayAsObservable().AsUnitObservable();
    }
}
