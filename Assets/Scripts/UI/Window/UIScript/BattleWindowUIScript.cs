using GameBase;
using PM.Enum.Battle;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-Battle")]
public class BattleWindowUIScript : DummyWindowBase
{
    [SerializeField] protected TextMeshProUGUI _turnText;
    [SerializeField] protected TextMeshProUGUI _waveText;
    [SerializeField] protected TextMeshProUGUI _skillNameText;
    [SerializeField] protected TextMeshProUGUI _actionDescriptionTitleText;
    [SerializeField] protected TextMeshProUGUI _actionDescriptionContentText;
    [SerializeField] protected List<BattleMonsterBase> _playerMonsterBaseList;
    [SerializeField] protected List<BattleMonsterBase> _enemyMonsterBaseList;
    [SerializeField] protected Transform _fxParent;
    [SerializeField] protected Transform _battleMonsterInfoItemBase;
    [SerializeField] protected GameObject _skillNameBase;
    [SerializeField] protected GameObject _actionDescriptionBase;
    [SerializeField] protected Button _pauseButton;

    private UserMonsterPartyInfo userMonsterParty;
    private QuestMB quest;
    private IDisposable skillNameObservable;
    private IDisposable actionDescriptionObservable;

    public void Init(string userMonsterPartyId, long questId)
    {
        userMonsterParty = ApplicationContext.userData.userMonsterPartyList.First(u => u.id == userMonsterPartyId);
        quest = MasterRecord.GetMasterOf<QuestMB>().Get(questId);

        _pauseButton.OnClickIntentAsObservable()
            .Do(_ => TimeManager.Instance.Pause())
            .SelectMany(_ => BattlePauseDialogFactory.Create(new BattlePauseDialogRequest()))
            .Do(_ => TimeManager.Instance.SpeedBy1())
            .Subscribe();

        SetPlayerMonsterImage();
        SetTurnText(1);
        SetWaveText(1, quest.questWaveIdList.Count);
        SetBattleMonsterInfoItem(userMonsterParty);
    }

    private void SetPlayerMonsterImage()
    {
        userMonsterParty.userMonsterIdList.ForEach((userMonsterId, index) =>
        {
            var userMonster = ApplicationContext.userInventory.userMonsterList.FirstOrDefault(u => u.id == userMonsterId);
            if (userMonster != null) {
                var parent = _playerMonsterBaseList[index];
                var item = UIManager.Instance.CreateContent<BattleMonsterItem>(parent.transform);

                parent.SetBattleMonsterItem(item);
                item.Init(userMonster.monsterId, userMonster.customData.level);
            }
        });
    }

    public void SetEnemyMonsterImage(int waveCount)
    {
        _enemyMonsterBaseList.ForEach(monsterBase =>
        {
            foreach (Transform child in monsterBase.transform)
            {
                Destroy(child.gameObject);
            }
        });

        var waveIndex = waveCount - 1;
        var questWaveId = quest.questWaveIdList[waveIndex];
        var questWave = MasterRecord.GetMasterOf<QuestWaveMB>().Get(questWaveId);

        questWave.questMonsterIdList.ForEach((questMonsterId, index) =>
        {
            var questMonster = MasterRecord.GetMasterOf<QuestMonsterMB>().GetAll().FirstOrDefault(m => m.id == questMonsterId);
            if(questMonster != null)
            {
                var parent = _enemyMonsterBaseList[index];
                var item = UIManager.Instance.CreateContent<BattleMonsterItem>(parent.transform);

                parent.SetBattleMonsterItem(item);
                item.Init(questMonster.monsterId, questMonster.level);
            }
        });
    }

    private void SetBattleMonsterInfoItem(UserMonsterPartyInfo userMonsterParty)
    {
        userMonsterParty.userMonsterIdList.ForEach(userMonsterId =>
        {
            var userMonster = ApplicationContext.userInventory.userMonsterList.FirstOrDefault(u => u.id == userMonsterId);
            var battleMonsterInfoItem = UIManager.Instance.CreateContent<BattleMonsterInfoItem>(_battleMonsterInfoItemBase);
            battleMonsterInfoItem.Set(userMonster);
        });
    }

    public void SetTurnText(int turnCount)
    {
        _turnText.text = $"Turn {turnCount}";
    }

    public void SetWaveText(int waveCount, int maxWaveCount)
    {
        _waveText.text = $"Wave {waveCount}/{maxWaveCount}";
    }

    public IObservable<Unit> PlayStartActionAnimationObservable(BattleMonsterInfo doBattleMonster, BattleActionType actionType)
    {
        const float SHOW_TIME = 2.0f;

        var skillNameAndDescription = GetSkillNameAndDescription(doBattleMonster, actionType);

        if (skillNameObservable != null)
        {
            skillNameObservable.Dispose();
            skillNameObservable = null;
        }
        if (actionDescriptionObservable != null)
        {
            actionDescriptionObservable.Dispose();
            actionDescriptionObservable = null;
        }

        skillNameObservable = Observable.ReturnUnit()
            .Do(_ =>
            {
                _skillNameText.text = skillNameAndDescription.skillName;
                _skillNameBase.SetActive(true);
            })
            .Delay(TimeSpan.FromSeconds(SHOW_TIME/1.5f))
            .DoOnCompleted(() => _skillNameBase.SetActive(false))
            .Subscribe();
        actionDescriptionObservable = Observable.ReturnUnit()
            .Do(_ =>
            {
                var actionName = GetActionName(actionType);
                _actionDescriptionTitleText.text = $"{actionName}の効果";
                _actionDescriptionContentText.text = $"{skillNameAndDescription.skillName}が発動！\n<color=\"yellow\">{skillNameAndDescription.skillDescription}";
                _actionDescriptionBase.SetActive(true);
            })
            .Delay(TimeSpan.FromSeconds(SHOW_TIME))
            .DoOnCompleted(() => _actionDescriptionBase.SetActive(false))
            .Subscribe();

        // アクション開始演出は終了を待たない
        return Observable.ReturnUnit();
    }

    public IObservable<Unit> PlayAttackAnimationObservable(BattleMonsterIndex doBattleMonsterIndex)
    {
        var isPlayer = doBattleMonsterIndex.isPlayer;
        var doMonsterBaseList = isPlayer ? _playerMonsterBaseList : _enemyMonsterBaseList;
        var doMonsterRT = doMonsterBaseList[doBattleMonsterIndex.index].battleMonsterItem.GetComponent<RectTransform>();
        return VisualFxManager.Instance.PlayStartAttackFxObservable(doMonsterRT, isPlayer);
    }

    public IObservable<Unit> PlayAttackAnimationObservable(BattleMonsterIndex doBattleMonsterIndex, List<BattleMonsterIndex> beDoneBattleMonsterIndexList)
    {
        var isPlayer = doBattleMonsterIndex.isPlayer;
        var doMonsterBaseList = isPlayer ? _playerMonsterBaseList : _enemyMonsterBaseList;
        var beDoneMonsterBaseList = isPlayer ? _enemyMonsterBaseList : _playerMonsterBaseList;

        var doMonsterRT = doMonsterBaseList[doBattleMonsterIndex.index].battleMonsterItem.GetComponent<RectTransform>();
        var beDoneMonsterBaseTransformList = beDoneMonsterBaseList
            .Where((battleMonsterBase, index) => beDoneBattleMonsterIndexList.Select(battleMonsterIndex => battleMonsterIndex.index).Contains(index))
            .Select(battleMonsterBase => battleMonsterBase.transform)
            .ToList();

        return VisualFxManager.Instance.PlayNormalAttackFxObservable(doMonsterRT, beDoneMonsterBaseTransformList, isPlayer);
    }

    public IObservable<Unit> PlayTakeDamageAnimationObservable(BattleMonsterIndex beDoneBattleMonsterIndex,long skillFxId, int damage, int currentHp)
    {
        var isPlayer = beDoneBattleMonsterIndex.isPlayer;
        var monsterBaseList = isPlayer ? _playerMonsterBaseList : _enemyMonsterBaseList;
        var monsterBase = monsterBaseList[beDoneBattleMonsterIndex.index];
        var slider = monsterBase.battleMonsterItem.GetComponent<BattleMonsterItem>().hpSlider;

        return VisualFxManager.Instance.PlayTakeDamageFxObservable(slider, monsterBase.transform,skillFxId, damage, Math.Max(0, currentHp));
    }

    public IObservable<Unit> PlayDieAnimationObservable(BattleMonsterIndex battleMonsterIndex)
    {
        var isPlayer = battleMonsterIndex.isPlayer;
        var monsterBaseList = isPlayer ? _playerMonsterBaseList : _enemyMonsterBaseList;
        var monster = monsterBaseList[battleMonsterIndex.index].battleMonsterItem.gameObject;
        monster.SetActive(false);
        return Observable.ReturnUnit();
    }
    
    public IObservable<Unit> PlayWaveTitleFxObservable(int currentWaveCount, int maxWaveCount){
        SetEnemyMonsterImage(currentWaveCount);
        SetWaveText(currentWaveCount, maxWaveCount);
        return VisualFxManager.Instance.PlayWaveTitleFxObservable(_fxParent, currentWaveCount, maxWaveCount);
    }

    public IObservable<Unit> PlayTurnFxObservable(int currentTurn)
    {
        SetTurnText(currentTurn);
        return Observable.ReturnUnit();
    }

    public IObservable<Unit> PlayWinAnimationObservable()
    {
        return VisualFxManager.Instance.PlayWinBattleFxObservable(_fxParent).Delay(TimeSpan.FromSeconds(1));
    }

    public IObservable<Unit> PlayLoseAnimationObservable()
    {
        return VisualFxManager.Instance.PlayLoseBattleFxObservable(_fxParent).Delay(TimeSpan.FromSeconds(1)); ;
    }

    private string GetActionName(BattleActionType actionType)
    {
        switch (actionType) {
            case BattleActionType.PassiveSkill:
                return "パッシブスキル";
            case BattleActionType.NormalSkill:
                return "通常スキル";
            case BattleActionType.UltimateSkill:
                return "アルティメットスキル";
            case BattleActionType.None:
            default:
                return "";
        }
    }
    private (string skillName, string skillDescription) GetSkillNameAndDescription(BattleMonsterInfo battleMonster, BattleActionType actionType)
    {
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);
        switch (actionType) {
            case BattleActionType.PassiveSkill:
                var passiveSkill = MasterRecord.GetMasterOf<PassiveSkillMB>().Get(monster.passiveSkillId);
                return (passiveSkill.name, passiveSkill.description);
            case BattleActionType.NormalSkill:
                var normalSkill = MasterRecord.GetMasterOf<NormalSkillMB>().Get(monster.normalSkillId);
                return (normalSkill.name, normalSkill.description);
            case BattleActionType.UltimateSkill:
                var ultimateSkill = MasterRecord.GetMasterOf<UltimateSkillMB>().Get(monster.ultimateSkillId);
                return (ultimateSkill.name, ultimateSkill.description);
            default:
                return ("", "");
        }
    }
}
