using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using PM.Enum.UI;
using PM.Enum.Battle;

[ResourcePath("UI/Dialog/Dialog-MonsterSkillDetail")]
public class MonsterSkillDetailDialogUIScript : DialogBase {
    [SerializeField] protected List<Text> _skillNameTextList;
    [SerializeField] protected List<Text> _skillDescriptionTextList;
    [SerializeField] protected List<GameObject> _skillBaseList;
    [SerializeField] protected Button _closeButton;

    private const string WHITE_COLOR_CODE = "#FFFFFF";
    private const string RED_COLOR_CODE = "#FF2F2F";
    private const string BLUE_COLOR_CODE = "#5EC1DB";
    private const string GRAY_COLOR_CODE = "#6D6D6D";

    private UserMonsterInfo userMonster;
    private BattleActionType battleActionType;

    public override void Init(DialogInfo info) {
        userMonster = (UserMonsterInfo)info.param["userMonster"];
        battleActionType = (BattleActionType)info.param["battleActionType"];
        var onClickClose = (Action)info.param["onClickClose"];

        _closeButton.OnClickIntentAsObservable()
            .SelectMany(_ => UIManager.Instance.CloseDialogObservable())
            .Do(_ => {
                if (onClickClose != null) {
                    onClickClose();
                    onClickClose = null;
                }
            })
            .Subscribe();

        RefreshUI();
    }

    private void RefreshUI() {
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(userMonster.monsterId);
        var maxMonsterLevel = MasterRecord.GetMasterOf<MaxMonsterLevelMB>().GetAll().Where(m => m.monsterRarity == monster.rarity).Max(m => m.maxMonsterLevel);
        var skillLevelUpTableList = MasterRecord.GetMasterOf<SkillLevelUpTableMB>().GetAll().Where(m => m.battleActionType == battleActionType && m.requiredMonsterLevel <= maxMonsterLevel).OrderBy(m => m.skillLevel).ToList();

        // àÍìxëSÇƒîÒï\é¶Ç…
        _skillBaseList.ForEach(g => g.SetActive(false));

        skillLevelUpTableList.ForEach((m, index) => {
            var skillData = GetSkillData(monster, battleActionType, m.skillLevel);
            var isOpened = userMonster.customData.level >= m.requiredMonsterLevel;
            var isCurrent = skillLevelUpTableList.Count == index + 1 && index != 0;
            var textColorCode =
                !isOpened ? GRAY_COLOR_CODE
                : isCurrent ? BLUE_COLOR_CODE
                : WHITE_COLOR_CODE;

            _skillNameTextList[index].text = $"<color=\"{textColorCode}\">{skillData.name} <color=\"{(isOpened ? textColorCode : RED_COLOR_CODE)}\">(Lv{m.requiredMonsterLevel}Ç≈âï˙{(isOpened ? "çœÇ›" : "")})</color></color>";
            _skillDescriptionTextList[index].text = $"<color=\"{textColorCode}\">{skillData.description}</color>";
            _skillBaseList[index].SetActive(true);
        });
    }

    private (string name, string description) GetSkillData(MonsterMB monster, BattleActionType battleActionType, int skillLevel) {
        switch (battleActionType) {
            case BattleActionType.NormalSkill:
                switch (skillLevel) {
                    case 1:
                        var level1NormalSkill = MasterRecord.GetMasterOf<NormalSkillMB>().Get(monster.level1NormalSkillId);
                        return (level1NormalSkill.name, level1NormalSkill.description);
                    case 2:
                        var level2NormalSkill = MasterRecord.GetMasterOf<NormalSkillMB>().Get(monster.level2NormalSkillId);
                        return (level2NormalSkill.name, level2NormalSkill.description);
                    case 3:
                        var level3NormalSkill = MasterRecord.GetMasterOf<NormalSkillMB>().Get(monster.level3NormalSkillId);
                        return (level3NormalSkill.name, level3NormalSkill.description);
                }
                break;
            case BattleActionType.UltimateSkill:
                switch (skillLevel) {
                    case 1:
                        var level1UltimateSkill = MasterRecord.GetMasterOf<UltimateSkillMB>().Get(monster.level1UltimateSkillId);
                        return (level1UltimateSkill.name, level1UltimateSkill.description);
                    case 2:
                        var level2UltimateSkill = MasterRecord.GetMasterOf<UltimateSkillMB>().Get(monster.level2UltimateSkillId);
                        return (level2UltimateSkill.name, level2UltimateSkill.description);
                    case 3:
                        var level3UltimateSkill = MasterRecord.GetMasterOf<UltimateSkillMB>().Get(monster.level3UltimateSkillId);
                        return (level3UltimateSkill.name, level3UltimateSkill.description);
                }
                break;
            case BattleActionType.PassiveSkill:
                switch (skillLevel) {
                    case 1:
                        var level1PassiveSkill = MasterRecord.GetMasterOf<PassiveSkillMB>().Get(monster.level1PassiveSkillId);
                        return (level1PassiveSkill.name, level1PassiveSkill.description);
                    case 2:
                        var level2PassiveSkill = MasterRecord.GetMasterOf<PassiveSkillMB>().Get(monster.level2PassiveSkillId);
                        return (level2PassiveSkill.name, level2PassiveSkill.description);
                    case 3:
                        var level3PassiveSkill = MasterRecord.GetMasterOf<PassiveSkillMB>().Get(monster.level3PassiveSkillId);
                        return (level3PassiveSkill.name, level3PassiveSkill.description);
                }
                break;
            default:
                break;
        }
        return ("", "");
    }

    public override void Back(DialogInfo info) {
    }
    public override void Close(DialogInfo info) {
    }
    public override void Open(DialogInfo info) {
    }
}
