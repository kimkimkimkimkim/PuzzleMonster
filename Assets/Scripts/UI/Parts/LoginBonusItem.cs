using GameBase;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-LoginBonusItem")]
public class LoginBonusItem : MonoBehaviour
{
    [SerializeField] Text _dayText;
    [SerializeField] Text _numText;
    [SerializeField] Image _frameImage;
    [SerializeField] Image _itemIconImage;
    [SerializeField] Image _checkImage;
    [SerializeField] Image _focusImage;
    [SerializeField] Sprite _pastFrameSprite;
    [SerializeField] Sprite _todayFrameSprite;
    [SerializeField] Sprite _futureFrameSprite;
    [SerializeField] Color _pastIconColor;
    [SerializeField] Color _normalIconColor;

    public IObservable<Unit> SetUIObservable(UserLoginBonusInfo userLoginBonus, int dayNum, LoginBonusItemState state)
    {
        
        var loginBonus = MasterRecord.GetMasterOf<LoginBonusMB>().Get(userLoginBonus.loginBonusId);
        var rewardItem = loginBonus.rewardItemList[dayNum - 1];

        return PMAddressableAssetUtil.GetIconImageSpriteObservable(rewardItem)
            .Do(iconSprite =>
            {
                var frameImageSprite =
                    state == LoginBonusItemState.Past ? _pastFrameSprite :
                    state == LoginBonusItemState.Today ? _todayFrameSprite :
                    _futureFrameSprite;
                var iconImageColor = dayNum < userLoginBonus.loginDateList.Count ? _pastIconColor : _normalIconColor;

                _dayText.text = $"DAY {dayNum}";
                _itemIconImage.sprite = iconSprite;
                _numText.text = $"{rewardItem.num}";
                _frameImage.sprite = _pastFrameSprite;
                _checkImage.gameObject.SetActive(state == LoginBonusItemState.Past);
                _focusImage.gameObject.SetActive(state == LoginBonusItemState.Today);
                _itemIconImage.color = iconImageColor;
            })
            .AsUnitObservable();
    }

    public enum LoginBonusItemState
    {
        Past,
        Today,
        Future,
    }
}