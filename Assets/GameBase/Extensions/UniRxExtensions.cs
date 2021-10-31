using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace GameBase
{
    public static class UniRxExtensions
    {
        public static IObservable<Unit> Connect<T>(this IObservable<T> source, IEnumerable<IObservable<T>> list)
        {
            return source.SelectMany(_ => Observable.WhenAll(Observable.Return(default(T)).Concat(list.ToArray()))).AsUnitObservable();
        }
    }
}