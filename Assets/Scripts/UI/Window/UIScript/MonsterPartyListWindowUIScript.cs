using System.Collections.Generic;
using System.Linq;
using GameBase;
using UniRx;
using UnityEngine;

[ResourcePath("UI/Window/Window-MonsterPartyList")]
public class MonsterPartyListWindowUIScript : WindowBase
{
    [SerializeField] protected InfiniteScroll _infiniteScroll;

    private bool isFirstTimeOpen = true; 
    private List<UserMonsterPartyInfo> userMonsterPartyList;

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        RefreshScroll();
    }

    private void RefreshScroll(bool enableAnimation = true)
    {
        _infiniteScroll.Clear();
        _infiniteScroll.enableAnimation = enableAnimation;

        userMonsterPartyList = Enumerable.Repeat<UserMonsterPartyInfo>(null, ConstManager.Battle.MAX_PARTY_NUM)
            .Select((_, index) =>
            {
                var partyId = index + 1;
                var userMonsterParty = ApplicationContext.userData.userMonsterPartyList.FirstOrDefault(u => u.partyId == partyId);
                if (userMonsterParty != null)
                {
                    return userMonsterParty;
                }
                else
                {
                    return new UserMonsterPartyInfo()
                    {
                        id = "",
                        partyId = partyId,
                        userMonsterIdList = new List<string>(),
                    };
                }
            })
            .ToList();

        if (userMonsterPartyList.Any()) _infiniteScroll.Init(userMonsterPartyList.Count, OnUpdateItem);
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((userMonsterPartyList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<MonsterPartyListScrollItem>();
        var userMonsterParty = userMonsterPartyList[index];
        var initialUserMonsterList = userMonsterParty.userMonsterIdList.Select(id => ApplicationContext.userInventory.userMonsterList.First(u => u.id == id)).ToList();
        var monsterIdList = initialUserMonsterList.Select(u => MasterRecord.GetMasterOf<MonsterMB>().Get(u.monsterId).id).ToList();

        scrollItem.SetMonsterImage(userMonsterParty.partyId, monsterIdList);
        scrollItem.SetOnClickAction(() =>
        {
            MonsterFormationWindowFactory.Create(new MonsterFormationWindowRequest()
            {
                partyId = userMonsterParty.partyId,
                userMontserList = ApplicationContext.userInventory.userMonsterList,
                initialUserMonsterList = initialUserMonsterList,
            }).Subscribe();
        });
    }

    public override void Open(WindowInfo info)
    {
        if (isFirstTimeOpen)
        {
            isFirstTimeOpen = false;
        }
        else
        {
            RefreshScroll(false);
        }
    }

    public override void Back(WindowInfo info)
    {
    }

    public override void Close(WindowInfo info)
    {
        base.Close(info);
    }
}
