using System.Collections.Generic;
using System.Linq;
using GameBase;
using UnityEngine;

[ResourcePath("UI/Window/Window-Gacha")]
public class GachaWindowUIScript : WindowBase
{
    [SerializeField] protected InfiniteScroll _infiniteScroll;

    private List<string> gachaBoxList;

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        gachaBoxList = new List<string>() { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };

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
        var animal = gachaBoxList[index];

        scrollItem.SetOnClickAction(() =>
        {

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
