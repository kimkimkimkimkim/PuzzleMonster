using UniRx;
using System;
using DG.Tweening;

namespace GameBase
{
    static public partial class DOTweenExtensions
    {
        static public IObservable<Tween> OnCompleteAsObservable(this Tween tweener)
        {
            return Observable.Create<Tween>(o =>
            {
                tweener.OnComplete(() =>
                {
                    o.OnNext(tweener);
                    o.OnCompleted();
                });
                return Disposable.Create(() => tweener.Kill());
            });
        }

        static public IObservable<Sequence> PlayAsObservable(this Sequence sequence)
        {
            return Observable.Create<Sequence>(o =>
            {
                sequence.OnComplete(() =>
                {
                    o.OnNext(sequence);
                    o.OnCompleted();
                });
                sequence.Play();
                return Disposable.Create(() => sequence.Kill());
            });
        }
    }
}
