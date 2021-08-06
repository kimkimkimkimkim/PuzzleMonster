using System;
using GameBase;
using PM.Enum.UI;
using UniRx;
using UnityEngine;

public class PMAddressableAssetUtil
{
    private const string ASSET_PATH_PREFIX = "Assets/Contents/PMResources";

    /// <summary>
    /// アイコン画像を取得する
    /// </summary>
    /// <param name="itemId">Master:マスタのID、 Enum:(int)Enumした値</param>
    public static IObservable<Sprite> GetIconImageSpriteObservable(IconImageType iconImageType, long itemId)
    {
        var address = ASSET_PATH_PREFIX + "/IconImage/" + iconImageType.ToString() + "/" + itemId.ToString() + ".png";
        return AddressableAssetController.LoadAssetAsObservable<Sprite>(address);
    }

    /// <summary>
    /// 指定した演出Prefabを取得する
    /// </summary>
    public static IObservable<T> InstantiateVisualFxItemObservable<T>(string name, Transform parent) where T : MonoBehaviour
    {
        var address = $"{ASSET_PATH_PREFIX}/VisualFx/{name}.prefab";
        return AddressableAssetController.InstantiateAsObservable<T>(address, parent);

    }
}
