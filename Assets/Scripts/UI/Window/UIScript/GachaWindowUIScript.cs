using System;
using System.Collections.Generic;
using System.Linq;
using GameBase;
using PM.Enum.Gacha;
using PM.Enum.UI;
using UniRx;
using UnityEngine;

[ResourcePath("UI/Window/Window-Gacha")]
public class GachaWindowUIScript : WindowBase
{
    [SerializeField] protected InfiniteScroll _infiniteScroll;

    private List<GachaBoxMB> gachaBoxList;

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        gachaBoxList = MasterRecord.GetMasterOf<GachaBoxMB>().GetAll().ToList();

        RefreshScroll();
    }

    private void RefreshScroll()
    {
        _infiniteScroll.Clear();

        if (gachaBoxList.Any()) _infiniteScroll.Init(gachaBoxList.Count, OnUpdateItem);
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((gachaBoxList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<GachaBoxScrollItem>();
        var gachaBox = gachaBoxList[index];
        var gachaBoxDetailList = MasterRecord.GetMasterOf<GachaBoxDetailMB>().GetAll().Where(m => m.gachaBoxId == gachaBox.id).ToList();
        
        gachaBoxDetailList.ForEach(gachaBoxDetail => {
            var gachaExecuteButton = UIManager.Instance.CreateContent<GachaExecuteButton>(scrollItem._executeButtonBase);
            var title = gachaBoxDetail.title;
            var cost = gachaBoxDetail.requiredItemList.First().num.ToString();

            gachaExecuteButton.SetText(title);
            gachaExecuteButton.SetCostText(cost);
            gachaExecuteButton.SetOnClickAction(() => OnClickGachaExecuteButtonAction(gachaBoxDetail));
        });
    }

    private void OnClickGachaExecuteButtonAction(GachaBoxDetailMB gachaBoxDetail) {
        CommonDialogFactory.Create(new CommonDialogRequest()
        {
            commonDialogType = CommonDialogType.NoAndYes,
            title = "開発用ガチャ",
            content = "オーブを0個使用してガチャを1回まわしますか？",
        })
            .Where(res => res.dialogResponseType == DialogResponseType.Yes)
            .SelectMany(_ => ExecuteGachaObservable())
            .SelectMany(res => GachaResultDialogFactory.Create(new GachaResultDialogRequest()
            {
                itemList = ItemUtil.GetItemMI(res.itemInstanceList)
            }))
            .Subscribe();
    }

    private IObservable<GrantItemsToUserApiResponse> ExecuteGachaObservable()
    {
        var itemIdList = new List<string>() { "Bundle3001001" };
        return ApiConnection.GrantItemsToUser(itemIdList);
    }

    public override void Open(WindowInfo info)
    {
    }

    public override void Back(WindowInfo info)
    {
    }

    public override void Close(WindowInfo info)
    {
        base.Close(info);
    }
}
