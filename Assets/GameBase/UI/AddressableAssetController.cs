using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GameBase
{
    public class AddressableAssetController
    {
        private static List<AsyncOperationHandle> assetList = new List<AsyncOperationHandle>();

        /// <summary>
        /// 指定されたアドレスにアセットが存在すれば取得、存在しなければその型のデフォルト値を返す
        /// </summary>
        public static IObservable<T> LoadAssetAsObservable<T>(string key)
        {
            if (AddressableAssetUtil.ContainsAddress(key))
            {
                return AddressableAssetUtil.GetDownloadSizeAsObservable(key)
                    .SelectMany(downloadSize =>
                    {
                        return Addressables.LoadAssetAsync<T>(key).AsObservable()
                            .Do(_ =>
                            {
                                // if (downloadSize > 0) KoiniwaLogger.Log(string.Format("asset download[{0}]:{1} ", key, downloadSize));
                            });
                    })
                    .Do(asset => assetList.Add(asset))
                    .Select(asset => asset.Result);
            }
            else
            {
                Debug.Log("Not found asset. address:" + key);
                return Observable.Return<T>(default);
            }
        }

        public static IObservable<T> InstantiateAsObservable<T>(string key, Transform parent, bool instantiateInWorldSpace = false) where T : MonoBehaviour
        {
            if (AddressableAssetUtil.ContainsAddress(key))
            {
                return AddressableAssetUtil.GetDownloadSizeAsObservable(key)
                    .SelectMany(downloadSize => {
                        return Addressables.InstantiateAsync(key, parent, instantiateInWorldSpace).AsObservable()
                            .Do(_ => {
                                // if (downloadSize > 0) KoiniwaLogger.Log(string.Format("asset download[{0}]:{1} ", key, downloadSize));
                            });
                    })
                    .Do(asset => assetList.Add(asset))
                    .Select(asset => asset.Result)
                    .Select(go => go.GetComponent<T>());
            }
            else
            {
                // KoiniwaLogger.Log(string.Format("Not found asset. address:{0}", key));
                return Observable.Return<T>(default);
            }
        }

        public static void Release()
        {
            assetList.ForEach(asset => Addressables.Release(asset));
            assetList.Clear();
        }
    }
}