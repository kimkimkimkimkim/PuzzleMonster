using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GameBase {

    /// <summary>
    /// クライアントで扱うAddressableAssetに関するユーティリティクラス
    /// </summary>
    class AddressableAssetUtil {

        /// <summary>
        /// AddressableAssetにキーが登録されているかのbool値を返します。
        /// </summary>
        public static bool ContainsAddress(string key) {
            var handle = Addressables.LoadResourceLocationsAsync(key);
            var isContain = handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null && handle.Result.Count >= 1;
            Addressables.Release(handle);// 存在判定のoperationHandleも都度リリースする必要がある

            // LoadResourceLocationAsyncはバンドルをビルドしておく必要がある？
            // 現状ローカルで使用しているので強制的にtrueを返す
            //return isContain;
            return true;
        }

        /// <summary>
        /// ダウンロードサイズを取得する。
        /// ダウンロード済みであれば0が返ってくる。
        /// </summary>
        public static IObservable<long> GetDownloadSizeAsObservable(string key) {
            return Addressables.GetDownloadSizeAsync(key).AsObservable()
                .Select(handle => {
                    var size = handle.Result;
                    Addressables.Release(handle);// サイズ取得のoperationHandleも都度リリースする必要がある
                    return size;
                });
        }

        public static IObservable<GameObject> InstantiateAsObservable(string key, Transform parent, bool instantiateInWorldSpace = false) {
            if (ContainsAddress(key)) {
                return GetDownloadSizeAsObservable(key)
                    .SelectMany(downloadSize => {
                        return Addressables.InstantiateAsync(key, parent, instantiateInWorldSpace).AsObservable()
                            .Do(_ => {
                                // if (downloadSize > 0) KoiniwaLogger.Log(string.Format("asset download[{0}]:{1} ", key, downloadSize));
                            });
                    })
                    .Select(asset => asset.Result);
            } else {
                // KoiniwaLogger.Log(string.Format("Not found asset. address:{0}", key));
                return Observable.Return<GameObject>(null);
            }
        }
    }
}