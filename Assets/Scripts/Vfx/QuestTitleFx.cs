using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using DG.Tweening;

public class QuestTitleFx : MonoBehaviour
{
    [SerializeField] Text _text;

    public IObservable<Unit> PlayFxObservable()
    {
        gameObject.SetActive(true);
        _text.SetAlpha(0);

        return DOTween.Sequence()
            .Append(DOVirtual.Float(0.0f, 1.0f, 1.0f, value => _text.SetAlpha(value)))
            .AppendInterval(2.0f)
            .Append(DOVirtual.Float(1.0f, 0.0f, 1.0f, value => _text.SetAlpha(value)))
            .OnCompleteAsObservable()
            .Do(_ => Destroy(gameObject))
            .AsUnitObservable();
    }
}
