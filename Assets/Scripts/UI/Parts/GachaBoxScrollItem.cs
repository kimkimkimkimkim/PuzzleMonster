using GameBase;
using PM.Enum.Gacha;
using PM.Enum.Item;
using PM.Enum.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-GachaBoxScrollItem")]
public class GachaBoxScrollItem : MonoBehaviour
{
    [SerializeField] protected Button _emissionRateButton;
    [SerializeField] protected Transform _executeButtonBase;
    [SerializeField] protected InfiniteScroll _infiniteScroll;
    [SerializeField] protected Text _limitText;
    [SerializeField] protected Image _gachaBannerImage;

    public Transform executeButtonBase
    { get { return _executeButtonBase; } }

    private long gachaBoxId;
    private List<ItemMI> itemList;
    private IDisposable onClickEmissionRateButtonObservable;

    public void SetGachaBannerImage(long gachaBoxId)
    {
        this.gachaBoxId = gachaBoxId;

        PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.GachaBanner, gachaBoxId)
            .Do(sprite =>
            {
                if (this.gachaBoxId == gachaBoxId) _gachaBannerImage.sprite = sprite;
            })
            .Subscribe();
    }

    public void SetLimitText(GachaBoxMB gachaBox)
    {
        _limitText.text = GetLimitText(gachaBox);
    }

    private string GetLimitText(GachaBoxMB gachaBox)
    {
        switch (gachaBox.openType)
        {
            case GachaOpenType.Schedule:
                var gachaSchedule = MasterRecord.GetMasterOf<GachaScheduleMB>().GetAll()
                    .Where(m => DateTimeUtil.GetDateFromMasterString(m.startDate) <= DateTimeUtil.Now && DateTimeUtil.Now < DateTimeUtil.GetDateFromMasterString(m.endDate))
                    .FirstOrDefault(m => m.gachaBoxId == gachaBox.id);
                return gachaSchedule != null ? $"{DateTimeUtil.GetDateFromMasterString(gachaSchedule.startDate).ToString("yyyy/MM/dd hh:mm:ss")}`{DateTimeUtil.GetDateFromMasterString(gachaSchedule.endDate).ToString("yyyy/MM/dd hh:mm:ss")}" : "";

            default:
                return "ŠúŒÀ‚È‚µ";
        }
    }

    public void SetOnClickEmissionRateButtonAction(Action action)
    {
        if (onClickEmissionRateButtonObservable != null)
        {
            onClickEmissionRateButtonObservable.Dispose();
            onClickEmissionRateButtonObservable = null;
        }

        onClickEmissionRateButtonObservable = _emissionRateButton.OnClickIntentAsObservable()
            .Do(_ => action())
            .Subscribe();
    }

    public void RefreshScroll(List<long> pickUpMonsterIdList)
    {
        _infiniteScroll.Clear();

        itemList = pickUpMonsterIdList
            .Select(id => MasterRecord.GetMasterOf<MonsterMB>().Get(id))
            .Select(m => new ItemMI()
            {
                itemType = ItemType.Monster,
                itemId = m.id,
            })
            .ToList();

        _infiniteScroll.Init(itemList.Count, OnUpdateItem);
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((itemList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<IconItem>();
        var itemMI = itemList[index];

        scrollItem.SetIcon(itemMI, isMaxStatus: true);
        scrollItem.SetShowMonsterDetailDialogAction(true);
    }
}