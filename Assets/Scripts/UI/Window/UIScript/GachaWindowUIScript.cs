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
        var gachaBoxDetail = MasterRecord.GetMasterOf<GachaBoxDetailMB>().GetAll().First(m => m.gachaBoxId == gachaBox.id);

        scrollItem.SetOnClickAction(() =>
        {
            CommonDialogFactory.Create(new CommonDialogRequest()
            {
                commonDialogType = CommonDialogType.NoAndYes,
                title = "開発用ガチャ",
                content = "オーブを0個使用してガチャを1回まわしますか？",
            })
                .Where(res => res.dialogResponseType == DialogResponseType.Yes)
                .SelectMany(_ => ApiConnection.DropItem(gachaBoxDetail.bundleId))
                .SelectMany(res => GachaResultDialogFactory.Create(new GachaResultDialogRequest()
                {
                    itemList = ItemUtil.GetItemMI(res.itemInstanceList)
                }))
                .Subscribe();
        });
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
