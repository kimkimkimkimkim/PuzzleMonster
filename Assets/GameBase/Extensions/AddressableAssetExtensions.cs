using UniRx;
using System;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GameBase
{
    static public class AddressableAssetExtension
    {
        public static IObservable<AsyncOperationHandle> AsObservable(this AsyncOperationHandle addressableAsync)
        {
            return Observable.FromEvent<AsyncOperationHandle>(
                h => addressableAsync.Completed += h,
                h => addressableAsync.Completed -= h
            ).Take(1);
        }

        public static IObservable<AsyncOperationHandle<TObject>> AsObservable<TObject>(this AsyncOperationHandle<TObject> addressableAsync)
        {
            return Observable.FromEvent<AsyncOperationHandle<TObject>>(
                h => addressableAsync.Completed += h,
                h => addressableAsync.Completed -= h
            ).Take(1);
        }
    }
}