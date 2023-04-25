using GameBase;
using PM.Enum.Item;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ResourcePath("UI/Window/Window-ItemBox")]
public class ItemBoxWindowUIScript : WindowBase
{
    [SerializeField] protected InfiniteScroll _infiniteScroll;

    private List<UserPropertyInfo> userPropertyList;

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        RefreshScroll();
    }

    private void RefreshScroll()
    {
        _infiniteScroll.Clear();

        userPropertyList = ApplicationContext.userData.userPropertyList;

        _infiniteScroll.Init(userPropertyList.Count, OnUpdateItem);
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((userPropertyList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<IconItem>();
        var userProperty = userPropertyList[index];

        scrollItem.SetIcon(ItemType.Property, userProperty.propertyId);
        scrollItem.SetNumText(userProperty.num.ToString());
        scrollItem.SetShowItemDetailDialogAction(true);
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