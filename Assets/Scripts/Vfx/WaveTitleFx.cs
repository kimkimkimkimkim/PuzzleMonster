using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using DG.Tweening;


public class WaveTitleFx : MonoBehaviour
{
    [SerializeField] Text _text;

    public IObservable<Unit> PlayFxObservable(int currentWaveCount, int maxWaveCount)
    {
        var distance = 10.0f;

        _text.SetAlpha(0);
        _text.text = $"Wave {currentWaveCount}/{maxWaveCount}";
        gameObject.SetActive(true);

        return DOTween.Sequence()
            .Append(transform.DOLocalMoveX(distance, 0.0f))
            .Append(DOVirtual.Float(0.0f, 1.0f, 1.0f, value => _text.SetAlpha(value)))
            .Join(transform.DOLocalMoveX(-distance, 1.0f))
            .AppendInterval(1.0f)
            .Append(DOVirtual.Float(1.0f, 0.0f, 1.0f, value => _text.SetAlpha(value)))
            .Join(transform.DOLocalMoveX(-distance, 1.0f))
            .OnCompleteAsObservable()
            .Do(_ => Destroy(gameObject))
            .AsUnitObservable();
    }
}
