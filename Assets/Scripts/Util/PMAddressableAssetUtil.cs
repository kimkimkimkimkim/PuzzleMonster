using System;
using System.Collections;
using System.Collections.Generic;
using GameBase;
using PM.Enum.UI;
using UnityEngine;

public class PMAddressableAssetUtil
{
    private const string ASSET_PATH_PREFIX = "Assets/Contents/PMResources";

    public static IObservable<Sprite> GetIconImageSpriteObservable(IconImageType iconImageType, long itemId)
    {
        var address = ASSET_PATH_PREFIX + "/IconImage/" + iconImageType.ToString() + "/" + itemId.ToString() + ".png";
        return AddressableAssetController.LoadAssetAsObservable<Sprite>(address);
    }
}
