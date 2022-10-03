using System;
using System.Collections.Generic;
using System.Linq;
using GameBase;
using PM.Enum.Gacha;
using PM.Enum.Item;
using PM.Enum.Monster;
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

        _infiniteScroll.Init(gachaBoxList.Count, OnUpdateItem);
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
        var monsterList = MasterRecord.GetMasterOf<MonsterMB>().GetAll().ToList();
        var pickUpMonsterNum = gachaBox.pickUpMonsterIdList.Count;
        var ssrMonsterWithoutPickUpNum = monsterList
            .Where(m => m.rarity == MonsterRarity.SSR)
            .Where(m => gachaBox.gachaBoxTypeList.Any(gachaBoxType => m.gachaBoxTypeList.Contains(gachaBoxType)))
            .Count() 
            - pickUpMonsterNum;
        var eachPickUpMonsterEmissionRate = gachaBox.normalEmissionPercentByRarity.pickUpSsrPercent;
        var eachSsrMonsterWithoutPickUpEmissionRate = (gachaBox.normalEmissionPercentByRarity.ssrAllPercent - (eachPickUpMonsterEmissionRate * pickUpMonsterNum)) / ssrMonsterWithoutPickUpNum;
        var pickUpMonsterEmissionRateTextList = gachaBox.pickUpMonsterIdList
            .Select(id => monsterList.First(m => m.id == id))
            .Select(m => $"{m.name}: {eachPickUpMonsterEmissionRate}％")
            .ToList();
        var content =
            "<color=\"\">■提供割合について</color>\n" +
            $"SSR: {gachaBox.normalEmissionPercentByRarity.ssrAllPercent}％\n" +
            $"{string.Join("\n", pickUpMonsterEmissionRateTextList)}\n" +
            $"その他: {eachSsrMonsterWithoutPickUpEmissionRate}％\n" +
            $"SR: {gachaBox.normalEmissionPercentByRarity.srAllPercent}％\n" +
            $"R: {gachaBox.normalEmissionPercentByRarity.rAllPercent}％";

        scrollItem.SetText(gachaBox.title);
        scrollItem.SetOnClickEmissionRateButtonAction(new Action(() =>
        {
            CommonDialogFactory.Create(new CommonDialogRequest()
            {
                commonDialogType = CommonDialogType.YesOnly,
                title = "提供割合について",
                content = content,
            }).Subscribe();
        }));
        scrollItem.RefreshScroll(gachaBox.pickUpMonsterIdList);
        gachaBoxDetailList.ForEach(gachaBoxDetail => {
            var gachaExecuteButton = UIManager.Instance.CreateContent<GachaExecuteButton>(scrollItem.executeButtonBase);
            var title = gachaBoxDetail.title;
            var canExecute = ConditionUtil.IsValid(ApplicationContext.userData, gachaBoxDetail.canExecuteConditionList);
            var cost = gachaBoxDetail.requiredItem.num;

            gachaExecuteButton.ShowGrayoutPanel(!canExecute);
            gachaExecuteButton.SetText(title);
            gachaExecuteButton.SetCostText(cost.ToString());
            gachaExecuteButton.SetOnClickAction(() => OnClickGachaExecuteButtonAction(gachaBoxDetail));
        });
    }

    private void OnClickGachaExecuteButtonAction(GachaBoxDetailMB gachaBoxDetail) {
        const float FADE_ANIMATION_TIME = 0.3f;

        var cost = gachaBoxDetail.requiredItem.num;
        var num = gachaBoxDetail.gachaExecuteType.Num();

        CommonDialogFactory.Create(new CommonDialogRequest()
        {
            commonDialogType = CommonDialogType.NoAndYes,
            title = "開発用ガチャ",
            content = $"オーブを{cost}個使用してガチャを{num}回まわしますか？",
        })
            .Where(res => res.dialogResponseType == DialogResponseType.Yes)
            .SelectMany(_ => ApiConnection.ExecuteGacha(gachaBoxDetail.id))
            .SelectMany(res =>
            {
                return FadeManager.Instance.PlayFadeAnimationObservable(1.0f, FADE_ANIMATION_TIME)
                    .SelectMany(_ =>
                    {
                        var gachaAnimation = UIManager.Instance.CreateContent<GachaAnimation>(UIManager.Instance.gachaAnimationParent);
                        return gachaAnimation.PlayGachaAnimationObservable();
                    }).Select(_ => res);
            })
            .SelectMany(res =>
            {
                FadeManager.Instance.PlayFadeAnimationObservable(0.0f, FADE_ANIMATION_TIME).Subscribe();
                return GachaResultWindowFactory.Create(new GachaResultWindowRequest() { itemList = res.rewardItemList});
            })
            .Subscribe();
    }

    private IObservable<GrantItemsToUserApiResponse> ExecuteGachaObservable(GachaBoxDetailMB gachaBoxDetail)
    {
        var itemId = "";
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
