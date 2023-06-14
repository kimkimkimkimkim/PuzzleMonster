using System.Collections.Generic;
using GameBase;
using PM.Enum.Item;
using UnityEngine;

[ResourcePath("UI/Parts/Parts-MonsterPartyListScrollItem")]
public class MonsterPartyListScrollItem : ScrollItem {
    [SerializeField] protected List<PartyMonsterIconItem> _partyMonsterIconItemList;

    private int partyId;

    public void SetTitle(string title) {
        _text.text = title;
    }

    public void SetMonsterImage(int partyId, List<long> monsterIdList) {
        _partyMonsterIconItemList.ForEach((icon, index) => {
            var monsterId = monsterIdList[index];

            if (monsterId <= 0) {
                icon.ShowIconItem(false);
                icon.ShowRarityImage(false);
                icon.ShowLevelText(false);
            } else {
                var itemMI = new ItemMI() { itemType = ItemType.Monster, itemId = monsterId };
                icon.ShowIconItem(true);
                icon.ShowRarityImage(true);
                icon.ShowLevelText(true);
                icon.SetIcon(itemMI);
            }
        });
    }
}
