using System;
using System.Collections.Generic;
using System.Linq;
using GameBase;
using PM.Enum.Gacha;
using PM.Enum.Item;
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

        gachaBoxList = MasterRecord.GetMasterOf<GachaBoxMB>().GetAll()
            .Where(gachaBox =>
            {
                // ガチャボックスに対応するガチャボックス詳細マスタの中に一つでも表示条件を満たしているものがあれば表示する
                var gachaBoxDetailList = MasterRecord.GetMasterOf<GachaBoxDetailMB>().GetAll().Where(gachaBoxDetail => gachaBoxDetail.gachaBoxId == gachaBox.id).ToList();
                return gachaBoxDetailList.Any(gachaBoxDetail => ConditionUtil.IsValid(ApplicationContext.userData, gachaBoxDetail.displayConditionList));
            })
            .ToList();

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
        var gachaBoxDetailList = MasterRecord.GetMasterOf<GachaBoxDetailMB>().GetAll()
            .Where(m => m.gachaBoxId == gachaBox.id)
            .Where(m => ConditionUtil.IsValid(ApplicationContext.userData, m.displayConditionList))
            .ToList();

        scrollItem.SetText(gachaBox.title);
        gachaBoxDetailList.ForEach(gachaBoxDetail => {
            var gachaExecuteButton = UIManager.Instance.CreateContent<GachaExecuteButton>(scrollItem._executeButtonBase);
            var title = gachaBoxDetail.title;
            var canExecute = ConditionUtil.IsValid(ApplicationContext.userData, gachaBoxDetail.canExecuteConditionList);
            var cost = gachaBoxDetail.requiredItemList.First().num; // ガチャに必要なアイテムが複数になることはない

            gachaExecuteButton.ShowGrayoutPanel(!canExecute);
            gachaExecuteButton.SetText(title);
            gachaExecuteButton.SetCostText(cost.ToString());
            gachaExecuteButton.SetOnClickAction(() => OnClickGachaExecuteButtonAction(gachaBoxDetail));
        });
    }

    private void OnClickGachaExecuteButtonAction(GachaBoxDetailMB gachaBoxDetail) {
        var cost = gachaBoxDetail.requiredItemList.First().num; // ガチャに必要なアイテムが複数になることはない
        var num = gachaBoxDetail.gachaExecuteType.Num();

        CommonDialogFactory.Create(new CommonDialogRequest()
        {
            commonDialogType = CommonDialogType.NoAndYes,
            title = "開発用ガチャ",
            content = $"オーブを{cost}個使用してガチャを{num}回まわしますか？",
        })
            .Where(res => res.dialogResponseType == DialogResponseType.Yes)
            .SelectMany(_ => ExecuteGachaObservable(gachaBoxDetail))
            .SelectMany(res =>
            {
                var itemList = ItemUtil.GetSeparatedItemMIList(res.itemInstanceList);
                itemList = itemList.Shuffle().ToList();
                return GachaResultDialogFactory.Create(new GachaResultDialogRequest()
                {
                    itemList = itemList,
                });
            })
            .Subscribe();
    }

    private IObservable<GrantItemsToUserApiResponse> ExecuteGachaObservable(GachaBoxDetailMB gachaBoxDetail)
    {
        var itemId = ItemUtil.GetItemId(ItemType.Bundle, gachaBoxDetail.bundleId);
        var itemIdList = new List<string>() { itemId };
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
