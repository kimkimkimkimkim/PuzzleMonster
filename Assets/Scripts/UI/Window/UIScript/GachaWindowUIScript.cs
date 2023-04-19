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
public class GachaWindowUIScript : WindowBase {
    [SerializeField] protected InfiniteScroll _infiniteScroll;

    private List<GachaBoxMB> gachaBoxList;

    public override void Init(WindowInfo info) {
        base.Init(info);

        RefreshScroll();
    }

    private void RefreshScroll() {
        _infiniteScroll.Clear();

        gachaBoxList = MasterRecord.GetMasterOf<GachaBoxMB>().GetAll()
            .Where(gachaBox => IsOpened(gachaBox))
            .ToList();

        _infiniteScroll.Init(gachaBoxList.Count, OnUpdateItem);
    }

    private bool IsOpened(GachaBoxMB gachaBox) {
        switch (gachaBox.openType) {
            case GachaOpenType.Schedule:
                // 対象のスケジュールが期間内なら表示する
                var gachaSchedule = MasterRecord.GetMasterOf<GachaScheduleMB>().GetAll().FirstOrDefault(m => m.gachaBoxId == gachaBox.id);
                if (gachaSchedule != null) {
                    return DateTimeUtil.GetDateFromMasterString(gachaSchedule.startDate) <= DateTimeUtil.Now && DateTimeUtil.Now < DateTimeUtil.GetDateFromMasterString(gachaSchedule.endDate);
                } else {
                    return false;
                }
            case GachaOpenType.Condition:
                // ガチャボックスに対応するガチャボックス詳細マスタの中に一つでも表示条件を満たしているものがあれば表示する
                var gachaBoxDetailList = MasterRecord.GetMasterOf<GachaBoxDetailMB>().GetAll().Where(gachaBoxDetail => gachaBoxDetail.gachaBoxId == gachaBox.id).ToList();
                return gachaBoxDetailList.Any(gachaBoxDetail => ConditionUtil.IsValid(ApplicationContext.userData, gachaBoxDetail.displayConditionList));
            default:
                return false;
        }
    }

    private void OnUpdateItem(int index, GameObject item) {
        if ((gachaBoxList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<GachaBoxScrollItem>();
        var gachaBox = gachaBoxList[index];
        var gachaBoxDetailList = MasterRecord.GetMasterOf<GachaBoxDetailMB>().GetAll()
            .Where(m => m.gachaBoxId == gachaBox.id)
            .Where(m => ConditionUtil.IsValid(ApplicationContext.userData, m.displayConditionList))
            .ToList();

        scrollItem.SetText(gachaBox.title);
        scrollItem.SetOnClickEmissionRateButtonAction(new Action(() => {
            GachaEmissionRateDialogFactory.Create(new GachaEmissionRateDialogRequest() {
                gachaBoxId = gachaBox.id,
            }).Subscribe();
        }));
        scrollItem.RefreshScroll(gachaBox.pickUpMonsterIdList);

        // ガチャ実行ボタンの生成
        foreach (Transform n in scrollItem.executeButtonBase.transform) GameObject.Destroy(n.gameObject);
        gachaBoxDetailList.ForEach(gachaBoxDetail => {
            var gachaExecuteButton = UIManager.Instance.CreateContent<GachaExecuteButton>(scrollItem.executeButtonBase);
            var title = gachaBoxDetail.title;
            var canExecute = ConditionUtil.IsValid(ApplicationContext.userData, gachaBoxDetail.canExecuteConditionList);
            var cost = gachaBoxDetail.requiredItem.num;
            var requiredItem = gachaBoxDetail.requiredItem;
            var possessedRequiredItemNum = ClientItemUtil.GetPossessedNum(requiredItem.itemType, requiredItem.itemId);
            var enoughRequiredItem = possessedRequiredItemNum >= cost;

            gachaExecuteButton.ShowGrayoutPanel(!canExecute || !enoughRequiredItem);
            gachaExecuteButton.SetText(title);
            gachaExecuteButton.SetCostText(cost.ToString());
            gachaExecuteButton.SetCostIcon(requiredItem);
            gachaExecuteButton.SetOnClickAction(() => OnClickGachaExecuteButtonAction(gachaBoxDetail));
        });
    }

    private void OnClickGachaExecuteButtonAction(GachaBoxDetailMB gachaBoxDetail) {
        const float FADE_ANIMATION_TIME = 0.3f;

        var name = ClientItemUtil.GetName(gachaBoxDetail.requiredItem);
        var cost = gachaBoxDetail.requiredItem.num;
        var num = gachaBoxDetail.gachaExecuteType.Num();

        CommonDialogFactory.Create(new CommonDialogRequest() {
            commonDialogType = CommonDialogType.NoAndYes,
            title = "確認",
            content = $"{name}を{cost}個使用してガチャを{num}回まわしますか？",
        })
            .Where(res => res.dialogResponseType == DialogResponseType.Yes)
            .SelectMany(_ => {
                var gachaBox = MasterRecord.GetMasterOf<GachaBoxMB>().Get(gachaBoxDetail.gachaBoxId);
                if (IsOpened(gachaBox)) {
                    return Observable.Return(true);
                } else {
                    var title = "確認";
                    var content = "ガチャ開催条件を満たしていません";
                    return CommonDialogFactory.Create(new CommonDialogRequest() { commonDialogType = CommonDialogType.YesOnly, title = title, content = content })
                        .Do(res => RefreshScroll())
                        .Select(res => false);
                }
            })
            .Where(isContinued => isContinued)
            .SelectMany(_ => ApiConnection.ExecuteGacha(gachaBoxDetail.id))
            .SelectMany(res => {
                return FadeManager.Instance.PlayFadeAnimationObservable(1.0f, FADE_ANIMATION_TIME)
                    .SelectMany(_ => {
                        var gachaAnimation = UIManager.Instance.CreateContent<GachaAnimation>(UIManager.Instance.gachaAnimationParent);
                        return gachaAnimation.PlayGachaAnimationObservable();
                    })
                    .Do(_ => RefreshScroll())
                    .Select(_ => res);
            })
            .SelectMany(res => {
                FadeManager.Instance.PlayFadeAnimationObservable(0.0f, FADE_ANIMATION_TIME).Subscribe();
                return GachaResultWindowFactory.Create(new GachaResultWindowRequest() { itemList = res.rewardItemList });
            })
            .Subscribe();
    }

    private IObservable<GrantItemsToUserApiResponse> ExecuteGachaObservable(GachaBoxDetailMB gachaBoxDetail) {
        var itemId = "";
        var itemIdList = new List<string>() { itemId };
        return ApiConnection.GrantItemsToUser(itemIdList);
    }

    public override void Open(WindowInfo info) {
    }

    public override void Back(WindowInfo info) {
    }

    public override void Close(WindowInfo info) {
        base.Close(info);
    }
}
