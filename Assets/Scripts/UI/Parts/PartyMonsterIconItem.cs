using GameBase;
using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-PartyMonsterIconItem")]
public class PartyMonsterIconItem : IconItem
{
    [SerializeField] protected Text _titleText;
    [SerializeField] protected IconItem _iconItem;

    private UserMonsterInfo userMonster;

    public new void SetIcon(ItemMI item, bool showNumTextAtOne = false, bool isMaxStatus = false)
    {
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(item.itemId);
        userMonster = ApplicationContext.userData.userMonsterList.FirstOrDefault(u => u.monsterId == item.itemId);
        _iconItem.SetIcon(item, showNumTextAtOne, isMaxStatus);
    }

    public new void ShowRarityImage(bool isShow)
    {
        _iconItem.ShowRarityImage(isShow);
    }

    public new void ShowLevelText(bool isShow)
    {
        _iconItem.ShowLevelText(isShow);
    }

    public void ShowIconItem(bool isShow)
    {
        _iconItem.gameObject.SetActive(isShow);
    }

    public new void SetShowMonsterDetailDialogAction(bool isSet)
    {
        if (userMonster == null)
        {
            SetLongClickAction(false);
            return;
        }

        var action = new Action(() => { MonsterDetailDialogFactory.Create(new MonsterDetailDialogRequest() { userMonster = userMonster, canStrength = false }).Subscribe(); });
        SetLongClickAction(isSet, action);
    }
}