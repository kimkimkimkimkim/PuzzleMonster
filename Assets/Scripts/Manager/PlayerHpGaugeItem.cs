using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHpGaugeItem : MonoBehaviour
{
    [SerializeField] protected Image _hpGaugeImage;
    [SerializeField] protected Text _hpGaugeText;

    private int maxHp;
    private Sequence hpGaugeAnimationSequence;

    public void Init(int maxHp)
    {
        this.maxHp = maxHp;
        _hpGaugeImage.fillAmount = 1;
        SetHpText(maxHp);
    }

    public IObservable<Unit> PlayHpGaugeAnimation(int enemyIndex,int playerHp)
    {
        const float ANIMATION_TIME = 0.5f;

        var endGaugeValue = (float)playerHp / maxHp;
        var currentGaugeValue = _hpGaugeImage.fillAmount;
        if (hpGaugeAnimationSequence != null && hpGaugeAnimationSequence.IsActive() && hpGaugeAnimationSequence.IsPlaying())
        {
            hpGaugeAnimationSequence.Kill();
        }
        hpGaugeAnimationSequence = DOTween.Sequence()
            .Append(_hpGaugeImage.DOFillAmount(endGaugeValue, ANIMATION_TIME))
            .Join(DOVirtual.Float(GetCurrentHp(),playerHp,ANIMATION_TIME,(value) => SetHpText((int)value)));

        return hpGaugeAnimationSequence.PlayAsObservable().AsUnitObservable();
    }

    private void SetHpText(int hp)
    {
        _hpGaugeText.text = $"{hp}/{maxHp}";
    }

    private int GetCurrentHp()
    {
        return int.Parse(_hpGaugeText.text.Split('/')[0]);
    }
}
