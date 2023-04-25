using System.Collections.Generic;
using System.Linq;
using GameBase;
using PM.Enum.SortOrder;
using PM.Enum.UI;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-MonsterBox")]
public class MonsterBoxWindowUIScript : WindowBase
{
    [SerializeField] protected Button _sortOrderButton;
    [SerializeField] protected Text _sortOrderButtonText;
    [SerializeField] protected InfiniteScroll _infiniteScroll;

    private List<UserMonsterInfo> userMonsterList;
    private List<UserMonsterInfo> sortedUserMonsterList;

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        userMonsterList = (List<UserMonsterInfo>)info.param["userMonsterList"];

        _sortOrderButton.OnClickIntentAsObservable()
            .SelectMany(_ =>
            {
                var filterAttribute = SaveDataUtil.SortOrder.GetFilterAttributeMonsterBox();
                var sortOrderType = SaveDataUtil.SortOrder.GetSortOrderTypeMonsterBox();
                return SortOrderMonsterDialogFactory.Create(new SortOrderMonsterDialogRequest() { initialFilterAttribute = filterAttribute, initialSortOrderType = sortOrderType });
            })
            .Where(res => res.dialogResponseType == DialogResponseType.Yes)
            .Do(res =>
            {
                SaveDataUtil.SortOrder.SetFilterAttriuteMonsterBox(res.filterAttribute);
                SaveDataUtil.SortOrder.SetSortOrderTypeMonster(res.sortOrderType);
                RefreshScroll();
            })
            .Subscribe();

        RefreshScroll();
    }

    private void RefreshScroll()
    {
        _infiniteScroll.Clear();

        SortList();

        _infiniteScroll.Init(sortedUserMonsterList.Count, OnUpdateItem);
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((sortedUserMonsterList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<MonsterBoxScrollItem>();
        var userMonster = sortedUserMonsterList[index];
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(userMonster.monsterId);

        scrollItem.SetGradeImage(userMonster.customData.grade);
        scrollItem.SetUI(userMonster.monsterId, monster.rarity, monster.attribute, userMonster.customData.level);
        scrollItem.SetOnClickAction(() =>
        {
            MonsterDetailDialogFactory.Create(new MonsterDetailDialogRequest() { userMonster = userMonster })
                .Where(res => res.isNeedRefresh)
                .Do(_ =>
                {
                    userMonsterList = ApplicationContext.userData.userMonsterList;
                    RefreshScroll();
                })
                .Subscribe();
        });
    }

    private void SortList()
    {
        var filterAttribute = SaveDataUtil.SortOrder.GetFilterAttributeMonsterBox();
        var sortOrderType = SaveDataUtil.SortOrder.GetSortOrderTypeMonsterBox();

        // ボタンのUI更新
        var sortOrderButtonText = "";
        sortOrderButtonText += sortOrderType.Name();
        sortOrderButtonText += $"\n<size=20>{(!filterAttribute.Any() ? "<color=\"#FFFFFF\">絞り込みなし</color>" : "<color=\"#F4D487\">絞り込みあり</color>")}</size>";
        _sortOrderButtonText.text = sortOrderButtonText;

        // 絞り込み
        var filteredUserMonsterList = userMonsterList.Where(u =>
        {
            var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(u.monsterId);
            return !filterAttribute.Any() || filterAttribute.Contains(monster.attribute);
        }).ToList();

        // 並び変え
        IOrderedEnumerable<UserMonsterInfo> orderedEnumerable;
        switch (sortOrderType)
        {
            case SortOrderTypeMonster.Id:
                orderedEnumerable = filteredUserMonsterList.OrderByDescending(u => u.monsterId);
                break;

            case SortOrderTypeMonster.Attribute:
                orderedEnumerable = filteredUserMonsterList.OrderBy(u =>
                {
                    var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(u.monsterId);
                    return (int)monster.attribute;
                });
                break;

            case SortOrderTypeMonster.Rarity:
                orderedEnumerable = filteredUserMonsterList.OrderByDescending(u =>
                {
                    var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(u.monsterId);
                    return (int)monster.rarity;
                });
                break;

            case SortOrderTypeMonster.Level:
                orderedEnumerable = filteredUserMonsterList.OrderByDescending(u => u.customData.level);
                break;

            case SortOrderTypeMonster.Grade:
                orderedEnumerable = filteredUserMonsterList.OrderByDescending(u => u.customData.grade);
                break;

            case SortOrderTypeMonster.Luck:
                orderedEnumerable = filteredUserMonsterList.OrderByDescending(u => u.customData.luck);
                break;

            case SortOrderTypeMonster.Hp:
                orderedEnumerable = filteredUserMonsterList.OrderByDescending(u =>
                {
                    var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(u.monsterId);
                    return MonsterUtil.GetMonsterStatus(monster, u.customData.level).hp;
                });
                break;

            case SortOrderTypeMonster.Attack:
                orderedEnumerable = filteredUserMonsterList.OrderByDescending(u =>
                {
                    var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(u.monsterId);
                    return MonsterUtil.GetMonsterStatus(monster, u.customData.level).attack;
                });
                break;

            case SortOrderTypeMonster.Defense:
                orderedEnumerable = filteredUserMonsterList.OrderByDescending(u =>
                {
                    var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(u.monsterId);
                    return MonsterUtil.GetMonsterStatus(monster, u.customData.level).defense;
                });
                break;

            case SortOrderTypeMonster.Speed:
                orderedEnumerable = filteredUserMonsterList.OrderByDescending(u =>
                {
                    var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(u.monsterId);
                    return MonsterUtil.GetMonsterStatus(monster, u.customData.level).speed;
                });
                break;

            default:
                orderedEnumerable = filteredUserMonsterList.OrderByDescending(u => u.monsterId);
                break;
        }

        sortedUserMonsterList = orderedEnumerable.ThenByDescending(u => u.monsterId).ToList();
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