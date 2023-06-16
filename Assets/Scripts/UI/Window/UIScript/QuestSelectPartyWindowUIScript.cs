using GameBase;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Linq;
using PM.Enum.UI;
using PM.Enum.Quest;

[ResourcePath("UI/Window/Window-QuestSelectParty")]
public class QuestSelectPartyWindowUIScript : MonsterFormationBaseWindowUIScript {
    [SerializeField] protected Text _titleText;
    [SerializeField] protected Button _okButton;

    private QuestMB quest;

    public override void Init(WindowInfo info) {
        base.Init(info);

        var questId = (long)info.param["questId"];
        quest = MasterRecord.GetMasterOf<QuestMB>().Get(questId);

        _titleText.text = quest.name;

        _okButton.OnClickIntentAsObservable()
            .SelectMany(res => {
                var isValidDisplayCondition = ConditionUtil.IsValid(ApplicationContext.userData, quest.displayConditionList);
                var isValidExecuteCondition = ConditionUtil.IsValid(ApplicationContext.userData, quest.canExecuteConditionList);
                var questCategory = MasterRecord.GetMasterOf<QuestCategoryMB>().Get(quest.questCategoryId);
                var isEventQuest = questCategory.questType == QuestType.Event;
                var isHolding = MasterRecord.GetMasterOf<EventQuestScheduleMB>().GetAll().Any(m => {
                    return m.questCategoryId == questCategory.id && DateTimeUtil.GetDateFromMasterString(m.startDate) <= DateTimeUtil.Now && DateTimeUtil.Now < DateTimeUtil.GetDateFromMasterString(m.endDate);
                });

                if (!currentUserMonsterParty.userMonsterIdList.Any(id => id != null)) {
                    return CommonDialogFactory.Create(new CommonDialogRequest() {
                        commonDialogType = CommonDialogType.YesOnly,
                        content = "モンスターを1体以上選択してください",
                    }).AsUnitObservable();
                } else if (isValidDisplayCondition && isValidExecuteCondition && (!isEventQuest || (isEventQuest && isHolding))) {
                    // バトルを実行
                    return Observable.ReturnUnit()
                        .SelectMany(_ => {
                            var userMonsterParty = ApplicationContext.userData.userMonsterPartyList.FirstOrDefault(u => u.id == currentUserMonsterParty.id);
                            if (userMonsterParty != null && userMonsterParty.IsSame(currentUserMonsterParty)) {
                                // 現在選択中のパーティ情報が存在し何も変更がなければそのまま進む
                                return Observable.ReturnUnit();
                            } else {
                                // 変更が加えられていた場合はユーザーデータを更新する
                                return ApiConnection.UpdateUserMosnterFormation(currentPartyIndex, currentUserMonsterParty.userMonsterIdList)
                                    .Do(resp => currentUserMonsterParty = resp.userMonsterParty)
                                    .AsUnitObservable();
                            }
                        })
                        .SelectMany(_ => BattleManager.Instance.StartBattleObservable(questId, currentUserMonsterParty.id));
                } else {
                    // クエスト実行条件を満たしていない場合はホーム画面に戻る
                    return CommonDialogFactory.Create(new CommonDialogRequest() {
                        title = "確認",
                        content = "クエスト実行条件を満たしていません\nホーム画面に戻ります",
                        commonDialogType = CommonDialogType.YesOnly,
                    })
                        .Do(_ => {
                            UIManager.Instance.CloseAllWindow(true);
                            HomeWindowFactory.Create(new HomeWindowRequest()).Subscribe();
                        })
                        .AsUnitObservable();
                }
            })
            .Subscribe();
    }

    public override void Open(WindowInfo info) {
    }

    public override void Back(WindowInfo info) {
    }

    public override void Close(WindowInfo info) {
        base.Close(info);
    }

    private enum GrayoutReason {
        None,
        NotExistsMonster,
        NotEnoughStamina,
        NotEnoughMaxStamina,
    }
}
