using GameBase;
using System.Collections.Generic;
using System.Linq;
using PM.Enum.Battle;
using System;

public class BattleDataProcessor
{
    private int currentWaveCount;
    private int currentTurnCount;
    private QuestMB quest;
    private List<BattleLogInfo> battleLogList = new List<BattleLogInfo>();
    private List<BattleMonsterInfo> playerBattleMonsterList = new List<BattleMonsterInfo>();
    private List<BattleMonsterInfo> enemyBattleMonsterList = new List<BattleMonsterInfo>();
    private BattleMonsterIndex doBattleMonsterIndex;
    private List<BeDoneBattleMonsterData> beDoneBattleMonsterDataList;
    private BattleMonsterActType doBattleMonsterActType;
    private WinOrLose currentWinOrLose;
    private BattlePhase currentBattlePhase;
    private long skillFxId;

    private void Init(UserMonsterPartyInfo userMonsterParty, QuestMB quest)
    {
        this.quest = quest;

        skillFxId = 0;
        currentWaveCount = 1;
        currentTurnCount = 1;
        currentWinOrLose = WinOrLose.Continue;
        doBattleMonsterIndex = null;
        beDoneBattleMonsterDataList = new List<BeDoneBattleMonsterData>();

        SetPlayerBattleMonsterList(userMonsterParty);
        RefreshEnemyBattleMonsterList(currentWaveCount);
    }

    public List<BattleLogInfo> GetBattleLogList(UserMonsterPartyInfo userMonsterParty, QuestMB quest)
    {
        Init(userMonsterParty, quest);

        ActivatePassiveSkillIfNeeded(SkillTriggerType.OnBattleStart);

        // 最初にウェーブ移動とターン移動のログを入れておく
        AddBattleLog(BattleLogType.MoveWave);
        AddBattleLog(BattleLogType.MoveTurn);

        // 勝敗が決まるまで続ける
        while (currentWinOrLose == WinOrLose.Continue)
        {
            ExecuteBattlePhase();
        }

        return battleLogList;
    }

    private void ExecuteBattlePhase()
    {
        switch (currentBattlePhase)
        {
            case BattlePhase.SetDoMonster:
                ExecuteSetDoMonsterPhase();
                break;
            case BattlePhase.MoveNextWave:
                ExecuteMoveNextWavePhase();
                break;
            case BattlePhase.MoveNextTurn:
                ExecuteMoveNextTurnPhase();
                break;
            case BattlePhase.SetDoSkill:
                ExecuteSetDoSkillPhase();
                break;
            case BattlePhase.ActivateSkill:
                ExecuteActivateSkillPhase();
                break;
            case BattlePhase.ReflectSkill:
                ExecuteReflectSkillPhase();
                break;
            case BattlePhase.JudgeBattleEnd:
                ExecuteJudgeBattleEndPhase();
                break;
        }

        currentBattlePhase = GetNextBattlePhase(currentBattlePhase);
    }

    /// <summary>
    /// 次行動するモンスターのモンスターインデックスを設定する
    /// nullの場合もある
    /// </summary>
    private void ExecuteSetDoMonsterPhase()
    {
        // 敵味方含めたバトルモンスターリストを取得
        var battleMonsterList = new List<BattleMonsterInfo>(playerBattleMonsterList);
        battleMonsterList.AddRange(enemyBattleMonsterList);

        // まだ行動していないかつスピードが一番早いモンスターを取得
        var battleMonster = battleMonsterList.Where(m => !m.isActed && m.currentHp > 0).OrderByDescending(m => m.currentSpeed).FirstOrDefault();
        doBattleMonsterIndex = battleMonster?.index;
    }

    /// <summary>
    /// 必要であれば次のウェーブに進む
    /// </summary>
    private void ExecuteMoveNextWavePhase()
    {
        // 敵が全滅していたら次のウェーブへ
        if (enemyBattleMonsterList.All(m => m.currentHp <= 0))
        {
            currentWaveCount++;
            playerBattleMonsterList.ForEach(m => m.isActed = false);
            enemyBattleMonsterList.ForEach(m => m.isActed = false);
            doBattleMonsterIndex = null;
            beDoneBattleMonsterDataList = new List<BeDoneBattleMonsterData>();
            RefreshEnemyBattleMonsterList(currentWaveCount);

            // ログ追加
            AddBattleLog(BattleLogType.MoveWave);
        }
    }

    /// <summary>
    /// 必要であれば次のターンに進む
    /// </summary>
    private void ExecuteMoveNextTurnPhase()
    {
        // 次行動するモンスターがいなければ次のターンへ
        if (doBattleMonsterIndex == null)
        {
            currentTurnCount++;
            playerBattleMonsterList.ForEach(m => m.isActed = false);
            enemyBattleMonsterList.ForEach(m => m.isActed = false);
            doBattleMonsterIndex = null;
            beDoneBattleMonsterDataList = new List<BeDoneBattleMonsterData>();

            // ログ追加
            AddBattleLog(BattleLogType.MoveTurn);
        }
    }

    /// <summary>
    /// 次行動するモンスターの使用するスキルを決める
    /// </summary>
    private void ExecuteSetDoSkillPhase()
    {
        if (doBattleMonsterIndex == null) return;

        // CTが最大まで溜まっていればアルティメットスキルそうでなければ通常スキルを設定
        var doBattleMonster = GetBattleMonster(doBattleMonsterIndex);
        doBattleMonsterActType = doBattleMonster.currentEnergy >= ConstManager.Battle.MAX_ENERGY_VALUE ? BattleMonsterActType.UltimateSkill : BattleMonsterActType.NormalSkill;
    }

    /// <summary>
    /// スキルを発動する
    /// </summary>
    private void ExecuteActivateSkillPhase()
    {
        if (doBattleMonsterIndex == null) return;

        var doBattleMonster = GetBattleMonster(doBattleMonsterIndex);
        doBattleMonster.isActed = true;

        switch (doBattleMonsterActType) {
            case BattleMonsterActType.PassiveSkill:
                break;
            case BattleMonsterActType.NormalSkill:
                doBattleMonster.currentEnergy += ConstManager.Battle.ENERGY_RISE_VALUE_ON_ACT;
                break;
            case BattleMonsterActType.UltimateSkill:
                doBattleMonster.currentEnergy = 0;
                break;
        }

        // ログを追加
        AddBattleLog(BattleLogType.StartAttack);
    }

    /// <summary>
    /// スキル効果を反映する
    /// </summary>
    private void ExecuteReflectSkillPhase()
    {
        if (doBattleMonsterIndex == null) return;

        var doBattleMonster = GetBattleMonster(doBattleMonsterIndex);
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(doBattleMonster.monsterId);
        var skillEffectList = new List<SkillEffectMI>();
        switch (doBattleMonsterActType)
        {
            case BattleMonsterActType.PassiveSkill:
                break;
            case BattleMonsterActType.NormalSkill:
                var normalSkill = MasterRecord.GetMasterOf<NormalSkillMB>().Get(monster.normalSkillId);
                skillEffectList = normalSkill.effectList.Select(m => (SkillEffectMI)m).ToList();
                break;
            case BattleMonsterActType.UltimateSkill:
                var ultimateSkill = MasterRecord.GetMasterOf<UltimateSkillMB>().Get(monster.ultimateSkillId);
                skillEffectList = ultimateSkill.effectList.Select(m => (SkillEffectMI)m).ToList();
                break;
        }

        // TODO: ダメージやログ
        skillEffectList.ForEach(skillEffect =>
        {
            var skillType = skillEffect.type;

            // TODO: 状態異常の実装
            if (skillType == SkillType.Condition) return;

            // 乱数
            var random = new Random();
            var coefficient = 1.0f - (((float)random.NextDouble() * 0.15f) - 0.075f);

            // 攻撃実数値
            var baseValue = GetBaseValue(doBattleMonster, skillEffect.valueTargetType);
            var value = (int)(baseValue * coefficient);

            // ダメージスキルなのであればマイナスにする
            if (skillType == SkillType.Damage) value = -1 * value;

            beDoneBattleMonsterDataList = GetBeDoneBattleMonsterDataList(skillEffect);
            beDoneBattleMonsterDataList.ForEach(beDoneBattleMonsterData =>
            {
                // TODO: 防御力計算
                var beDoneBattleMonster = GetBattleMonster(beDoneBattleMonsterData.battleMonsterIndex);
                beDoneBattleMonster.currentHp += value;
                beDoneBattleMonsterData.hpChanges = value;
            });

            // ログを追加
            AddBattleLog(BattleLogType.TakeDamage);
            ActivatePassiveSkillIfNeeded(SkillTriggerType.OnMeTakeDamageEnd);

            var dieBattleMonsterIndexList = beDoneBattleMonsterDataList
                .Where(d =>
                {
                    var battleMonster = GetBattleMonster(d.battleMonsterIndex);
                    return battleMonster.currentHp <= 0;
                })
                .Select(d => d.battleMonsterIndex)
                .ToList();
            if (dieBattleMonsterIndexList.Any())
            {
                AddDieBattleLog(dieBattleMonsterIndexList);
                ActivatePassiveSkillIfNeeded(SkillTriggerType.OnMeDeadEnd);
            }
        });

        // パッシブスキル発動
        ActivatePassiveSkillIfNeeded(SkillTriggerType.OnMeTurnEnd);
        switch (doBattleMonsterActType)
        {
            case BattleMonsterActType.NormalSkill:
                ActivatePassiveSkillIfNeeded(SkillTriggerType.OnMeNormalSkillEnd);
                break;
            case BattleMonsterActType.UltimateSkill:
                ActivatePassiveSkillIfNeeded(SkillTriggerType.OnMeUltimateSkillEnd);
                break;
        }
    }

    /// <summary>
    /// 勝敗を判定する
    /// </summary>
    private void ExecuteJudgeBattleEndPhase()
    {
        if (enemyBattleMonsterList.All(m => m.currentHp <= 0))
        {
            // 敵が全滅かつ最後のウェーブであれば勝利
            if (quest.questWaveIdList.Count == currentWaveCount)
            {
                currentWinOrLose = WinOrLose.Win;
                AddBattleLog(BattleLogType.Result);
                return;
            }
        }

        // 味方が全滅していたら負け
        if (playerBattleMonsterList.All(m => m.currentHp <= 0))
        {
            currentWinOrLose = WinOrLose.Lose;
            AddBattleLog(BattleLogType.Result);
            return;
        }

        // それ以外であれば続行
        currentWinOrLose = WinOrLose.Continue;
    }

    private void AddDieBattleLog(List<BattleMonsterIndex> dieBattleMonsterIndexList)
    {
        var stringList = dieBattleMonsterIndexList.Select(index =>
        {
            var battleMonster = GetBattleMonster(index);
            var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);
            return $"{monster.name}はたおれた";
        }).ToList();
        var log = string.Join("\n", stringList);
        var battleLog = GetCurrentBattleLog(BattleLogType.Die, log);

        // 死亡ログの対象モンスターは死亡したモンスターに上書きする
        battleLog.beDoneBattleMonsterDataList = dieBattleMonsterIndexList.Select(index => new BeDoneBattleMonsterData()
        {
            battleMonsterIndex = index,
        }).ToList();
        battleLogList.Add(battleLog);
    }

    private void AddBattleLog(BattleLogType type)
    {
        var log = "";

        switch (type)
        {
            case BattleLogType.StartAttack:
                var doBattleMonster = GetBattleMonster(doBattleMonsterIndex);
                var doMonster = MasterRecord.GetMasterOf<MonsterMB>().Get(doBattleMonster.monsterId);
                var skillName = "";
                switch (doBattleMonsterActType)
                {
                    case BattleMonsterActType.PassiveSkill:
                        break;
                    case BattleMonsterActType.NormalSkill:
                        var normalSkill = MasterRecord.GetMasterOf<NormalSkillMB>().Get(doMonster.normalSkillId);
                        skillName = normalSkill.name;
                        break;
                    case BattleMonsterActType.UltimateSkill:
                        var ultimateSkill = MasterRecord.GetMasterOf<UltimateSkillMB>().Get(doMonster.ultimateSkillId);
                        skillName = ultimateSkill.name;
                        break;
                }
                log = $"{doMonster.name}の「{skillName}」";
                break;
            case BattleLogType.TakeDamage:
                var stringList = beDoneBattleMonsterDataList.Select(d =>
                {
                    var battleMonster = GetBattleMonster(d.battleMonsterIndex);
                    var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);
                    return $"{monster.name}に{d.hpChanges}のダメージ";
                }).ToList();
                log = string.Join("\n", stringList);
                break;
            case BattleLogType.Die:
                log = $"{{beDone}}はたおれた";
                break;
            case BattleLogType.MoveWave:
                log = $"Wave{currentWaveCount} スタート";
                break;
            case BattleLogType.MoveTurn:
                log = $"Turn{currentTurnCount} スタート";
                break;
            case BattleLogType.Result:
                log = currentWinOrLose == WinOrLose.Win ? "WIN" : "LOSE";
                break;
            default:
                break;
        }

        var battleLog = GetCurrentBattleLog(type, log);
        battleLogList.Add(battleLog);
    }

    private BattleLogInfo GetCurrentBattleLog(BattleLogType type, string log)
    {
        return new BattleLogInfo()
        {
            type = type,
            playerBattleMonsterList = playerBattleMonsterList.Clone(),
            enemyBattleMonsterList = enemyBattleMonsterList.Clone(),
            doBattleMonsterIndex = doBattleMonsterIndex != null ? new BattleMonsterIndex(doBattleMonsterIndex) : null,
            beDoneBattleMonsterDataList = beDoneBattleMonsterDataList != null ? beDoneBattleMonsterDataList.Clone() : new List<BeDoneBattleMonsterData>(),
            waveCount = currentWaveCount,
            turnCount = currentTurnCount,
            winOrLose = currentWinOrLose,
            log = log,
            skillFxId = skillFxId,
        };
    }

    private float GetBaseValue(BattleMonsterInfo battleMonster, ValueTargetType valueTargetType)
    {
        switch (valueTargetType) {
            case ValueTargetType.MyCurrentHP:
                return battleMonster.currentHp;
            case ValueTargetType.MyCurrentAttack:
                return battleMonster.currentAttack;
            case ValueTargetType.MyCurrentDefense:
                return battleMonster.currentDefense;
            case ValueTargetType.MyCurrentHeal:
                return battleMonster.currentHeal;
            case ValueTargetType.MyCurrentSpeed:
                return battleMonster.currentSpeed;
            case ValueTargetType.MyMaxHp:
                return battleMonster.maxHp;
            case ValueTargetType.None:
            default:
                return 0.0f;
                
        }
    }

    private List<BeDoneBattleMonsterData> GetBeDoneBattleMonsterDataList(SkillEffectMI skillEffect)
    {
        var isDoMonsterPlayer = doBattleMonsterIndex.isPlayer;
        var allyBattleMonsterList = isDoMonsterPlayer ? playerBattleMonsterList : this.enemyBattleMonsterList;
        var enemyBattleMonsterList = isDoMonsterPlayer ? this.enemyBattleMonsterList : playerBattleMonsterList;
        allyBattleMonsterList = allyBattleMonsterList.Where(b => IsValid(b, skillEffect.activateConditionType)).ToList();
        enemyBattleMonsterList = enemyBattleMonsterList.Where(b => IsValid(b, skillEffect.activateConditionType)).ToList();

        var battleMonsterIndexList = new List<BattleMonsterIndex>();
        switch (skillEffect.skillTargetType)
        {
            case SkillTargetType.Myself:
                battleMonsterIndexList = new List<BattleMonsterIndex>() { doBattleMonsterIndex };
                break;
            case SkillTargetType.AllyAll:
                battleMonsterIndexList = allyBattleMonsterList.Select(b => b.index).ToList();
                break;
            case SkillTargetType.EnemyAll:
                battleMonsterIndexList = enemyBattleMonsterList.Select(b => b.index).ToList();
                break;
            case SkillTargetType.AllyAllRandom1:
                battleMonsterIndexList = allyBattleMonsterList.Shuffle().Take(1).Select(b => b.index).ToList();
                break;
            case SkillTargetType.AllyAllRandom2:
                battleMonsterIndexList = allyBattleMonsterList.Shuffle().Take(2).Select(b => b.index).ToList();
                break;
            case SkillTargetType.AllyAllRandom3:
                battleMonsterIndexList = allyBattleMonsterList.Shuffle().Take(3).Select(b => b.index).ToList();
                break;
            case SkillTargetType.AllyAllRandom4:
                battleMonsterIndexList = allyBattleMonsterList.Shuffle().Take(4).Select(b => b.index).ToList();
                break;
            case SkillTargetType.AllyAllRandom5:
                battleMonsterIndexList = allyBattleMonsterList.Shuffle().Take(5).Select(b => b.index).ToList();
                break;
            case SkillTargetType.EnemyAllRandom1:
                battleMonsterIndexList = enemyBattleMonsterList.Shuffle().Take(1).Select(b => b.index).ToList();
                break;
            case SkillTargetType.EnemyAllRandom2:
                battleMonsterIndexList = enemyBattleMonsterList.Shuffle().Take(2).Select(b => b.index).ToList();
                break;
            case SkillTargetType.EnemyAllRandom3:
                battleMonsterIndexList = enemyBattleMonsterList.Shuffle().Take(3).Select(b => b.index).ToList();
                break;
            case SkillTargetType.EnemyAllRandom4:
                battleMonsterIndexList = enemyBattleMonsterList.Shuffle().Take(4).Select(b => b.index).ToList();
                break;
            case SkillTargetType.EnemyAllRandom5:
                battleMonsterIndexList = enemyBattleMonsterList.Shuffle().Take(5).Select(b => b.index).ToList();
                break;
            case SkillTargetType.DoAttack:
                battleMonsterIndexList = new List<BattleMonsterIndex>() { doBattleMonsterIndex };
                break;
            case SkillTargetType.BeAttacked:
                battleMonsterIndexList = beDoneBattleMonsterDataList.Where(d => IsValid(d.battleMonsterIndex, skillEffect.activateConditionType)).Select(d => d.battleMonsterIndex).ToList();
                break;
            case SkillTargetType.AllyFrontAll:
                var allyFrontAll = allyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                // 前衛のモンスターが1体もいない場合は後衛全体を対象とする
                battleMonsterIndexList = allyFrontAll.Any() ? allyFrontAll : allyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                break;
            case SkillTargetType.AllyBackAll:
                var allyBackAll = allyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                // 後衛のモンスターが1体もいない場合は前衛全体を対象とする
                battleMonsterIndexList = allyBackAll.Any() ? allyBackAll : allyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                break;
            case SkillTargetType.EnemyFrontAll:
                var enemyFrontAll = enemyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                // 前衛のモンスターが1体もいない場合は後衛全体を対象とする
                battleMonsterIndexList = enemyFrontAll.Any() ? enemyFrontAll : enemyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                break;
            case SkillTargetType.EnemyBackAll:
                var enemyBackAll = enemyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                // 後衛のモンスターが1体もいない場合は前衛全体を対象とする
                battleMonsterIndexList = enemyBackAll.Any() ? enemyBackAll : enemyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                break;
            case SkillTargetType.AllyMostFront:
                battleMonsterIndexList = allyBattleMonsterList.OrderBy(b => b.index.index).Take(1).Select(b => b.index).ToList();
                break;
            case SkillTargetType.EnemyMostFront:
                battleMonsterIndexList = enemyBattleMonsterList.OrderBy(b => b.index.index).Take(1).Select(b => b.index).ToList();
                break;
            case SkillTargetType.AllyAllHPLowest1:
                battleMonsterIndexList = allyBattleMonsterList.OrderBy(b => b.currentHp).Take(1).Select(b => b.index).ToList();
                break;
            case SkillTargetType.AllyAllHPLowest2:
                battleMonsterIndexList = allyBattleMonsterList.OrderBy(b => b.currentHp).Take(2).Select(b => b.index).ToList();
                break;
            case SkillTargetType.AllyAllHPLowest3:
                battleMonsterIndexList = allyBattleMonsterList.OrderBy(b => b.currentHp).Take(3).Select(b => b.index).ToList();
                break;
            case SkillTargetType.AllyAllHPLowest4:
                battleMonsterIndexList = allyBattleMonsterList.OrderBy(b => b.currentHp).Take(4).Select(b => b.index).ToList();
                break;
            case SkillTargetType.EnemyAllHPLowest1:
                battleMonsterIndexList = enemyBattleMonsterList.OrderBy(b => b.currentHp).Take(1).Select(b => b.index).ToList();
                break;
            case SkillTargetType.EnemyAllHPLowest2:
                battleMonsterIndexList = enemyBattleMonsterList.OrderBy(b => b.currentHp).Take(2).Select(b => b.index).ToList();
                break;
            case SkillTargetType.EnemyAllHPLowest3:
                battleMonsterIndexList = enemyBattleMonsterList.OrderBy(b => b.currentHp).Take(3).Select(b => b.index).ToList();
                break;
            case SkillTargetType.EnemyAllHPLowest4:
                battleMonsterIndexList = enemyBattleMonsterList.OrderBy(b => b.currentHp).Take(4).Select(b => b.index).ToList();
                break;
            case SkillTargetType.Target:
                battleMonsterIndexList = beDoneBattleMonsterDataList.Where(d => IsValid(d.battleMonsterIndex, skillEffect.activateConditionType)).Select(d => d.battleMonsterIndex).ToList();
                break;
            case SkillTargetType.None:
            default:
                battleMonsterIndexList = new List<BattleMonsterIndex>();
                break;
        }
        return battleMonsterIndexList.Select(index => new BeDoneBattleMonsterData() { battleMonsterIndex = index }).ToList();
    }

    private bool IsValid(BattleMonsterIndex battleMonsterIndex, ActivateConditionType activateConditionType)
    {
        var battleMonster = GetBattleMonster(battleMonsterIndex);
        return IsValid(battleMonster, activateConditionType);
    }

    private bool IsValid(BattleMonsterInfo battleMonster, ActivateConditionType activateConditionType)
    {
        switch (activateConditionType)
        {
            case ActivateConditionType.Under50PercentMyHP:
                // HPが50%未満ならOK
                return battleMonster.currentHp < battleMonster.maxHp / 2;
            case ActivateConditionType.Alive:
                // HPが0より多ければOK
                return battleMonster.currentHp > 0;
            case ActivateConditionType.None:
            default:
                return false;
        }
    }

    /// <summary>
    /// 指定したモンスターとトリガータイプをもとにパッシブスキルを発動できるか否かを返す
    /// トリガーは正しいタイミングで呼ばれるので主語が正しいか判定
    /// </summary>
    private bool IsValid(BattleMonsterInfo battleMonster, SkillTriggerType skillTriggerType)
    {
        var doBattleMonster = GetBattleMonster(doBattleMonsterIndex);

        switch (skillTriggerType) {
            case SkillTriggerType.EveryTimeEnd:
                return true;
            case SkillTriggerType.OnBattleStart:
                return true;
            case SkillTriggerType.OnMeTurnEnd:
                return doBattleMonster.index.index == battleMonster.index.index;
            case SkillTriggerType.OnMeNormalSkillEnd:
                return doBattleMonster.index.index == battleMonster.index.index;
            case SkillTriggerType.OnMeUltimateSkillEnd:
                return doBattleMonster.index.index == battleMonster.index.index;
            case SkillTriggerType.OnMeTakeDamageEnd:
                return beDoneBattleMonsterDataList.Any(d => d.battleMonsterIndex.index == battleMonster.index.index);
            case SkillTriggerType.OnMeDeadEnd:
                return beDoneBattleMonsterDataList.Any(d => d.battleMonsterIndex.index == battleMonster.index.index);
            case SkillTriggerType.None:
            default:
                return false;
        }
    }

    private bool IsFront(BattleMonsterIndex battleMonsterIndex)
    {
        return ConstManager.Battle.FRONT_INDEX_LIST.Contains(battleMonsterIndex.index);
    }

    private bool IsBack(BattleMonsterIndex battleMonsterIndex)
    {
        return ConstManager.Battle.BACK_INDEX_LIST.Contains(battleMonsterIndex.index);
    }

    private void SetPlayerBattleMonsterList(UserMonsterPartyInfo userMonsterParty)
    {
        playerBattleMonsterList.Clear();
        userMonsterParty.userMonsterIdList.ForEach((userMonsterId, index) =>
        {
            var userMonster = ApplicationContext.userInventory.userMonsterList.FirstOrDefault(u => u.id == userMonsterId);
            if (userMonster != null)
            {
                var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(userMonster.monsterId);
                var battleMonster = BattleUtil.GetBattleMonster(monster, userMonster.customData.level, true, index);
                playerBattleMonsterList.Add(battleMonster);
            }
        });
    }

    private void RefreshEnemyBattleMonsterList(int waveCount)
    {
        var waveIndex = waveCount - 1;
        var questWaveId = quest.questWaveIdList[waveIndex];
        var questWave = MasterRecord.GetMasterOf<QuestWaveMB>().Get(questWaveId);

        enemyBattleMonsterList.Clear();
        questWave.questMonsterIdList.ForEach((questMonsterId, index) =>
        {
            var questMonster = MasterRecord.GetMasterOf<QuestMonsterMB>().GetAll().FirstOrDefault(m => m.id == questMonsterId);
            if (questMonster != null)
            {
                var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(questMonster.monsterId);
                var battleMonster = BattleUtil.GetBattleMonster(monster, questMonster.level, false, index);
                enemyBattleMonsterList.Add(battleMonster);
            }
        });
    }

    private BattleMonsterInfo GetBattleMonster(BattleMonsterIndex battleMonsterIndex)
    {
        var battleMonsterList = battleMonsterIndex.isPlayer ? playerBattleMonsterList : enemyBattleMonsterList;
        return battleMonsterList.FirstOrDefault(b => b.index.index == battleMonsterIndex.index);
    }

    /// <summary>
    /// 指定したトリガータイプであり発動可能なパッシブスキルがある場合は発動する
    /// </summary>
    private void ActivatePassiveSkillIfNeeded(SkillTriggerType triggerType)
    {
        var beforeDoBattleMonsterIndex = doBattleMonsterIndex;
        var beforeBeDoneBattleMonsterDataList = beDoneBattleMonsterDataList;

        // 敵味方合わせたモンスターリストを取得
        var battleMonsterList = new List<BattleMonsterInfo>(playerBattleMonsterList);
        battleMonsterList.AddRange(enemyBattleMonsterList);

        // 発動するパッシブスキルを取得
        var battleMonsterAndSkillEffectListSetList = battleMonsterList
            .Select(b =>
            {
                var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(b.monsterId);
                var passiveSkill = MasterRecord.GetMasterOf<PassiveSkillMB>().Get(monster.passiveSkillId);
                var activateSkillEffectList = passiveSkill.effectList.Where(effect => effect.triggerType == triggerType && IsValid(b, triggerType)).Select(effect => (SkillEffectMI)effect).ToList();
                return new BattleMonsterAndSkillEffectListSet()
                {
                    battleMonster = b,
                    skillEffectList = activateSkillEffectList,
                };
            })
            .Where(set => set.skillEffectList.Any())
            .ToList();

        // 発動
        battleMonsterAndSkillEffectListSetList.ForEach(set =>
        {
            doBattleMonsterIndex = set.battleMonster.index;
            AddBattleLog(BattleLogType.StartAttack);

            var doBattleMonster = GetBattleMonster(doBattleMonsterIndex);
            set.skillEffectList.ForEach(skillEffect =>
            {
                var skillType = skillEffect.type;

                // TODO: 状態異常の実装
                if (skillType == SkillType.Condition) return;

                // 乱数
                var random = new Random();
                var coefficient = 1.0f - (((float)random.NextDouble() * 0.15f) - 0.075f);

                // 攻撃実数値
                var baseValue = GetBaseValue(doBattleMonster, skillEffect.valueTargetType);
                var value = (int)(baseValue * coefficient);

                // ダメージスキルなのであればマイナスにする
                if (skillType == SkillType.Damage) value = -1 * value;

                beDoneBattleMonsterDataList = GetBeDoneBattleMonsterDataList(skillEffect);
                beDoneBattleMonsterDataList.ForEach(beDoneBattleMonsterData =>
                {
                    // TODO: 防御力計算
                    var beDoneBattleMonster = GetBattleMonster(beDoneBattleMonsterData.battleMonsterIndex);
                    beDoneBattleMonster.currentHp += value;
                    beDoneBattleMonsterData.hpChanges = value;
                });

                // ログを追加
                AddBattleLog(BattleLogType.TakeDamage);

                var dieBattleMonsterIndexList = beDoneBattleMonsterDataList
                    .Where(d =>
                    {
                        var battleMonster = GetBattleMonster(d.battleMonsterIndex);
                        return battleMonster.currentHp <= 0;
                    })
                    .Select(d => d.battleMonsterIndex)
                    .ToList();
                if (dieBattleMonsterIndexList.Any()) AddDieBattleLog(dieBattleMonsterIndexList);
            });
        });

        // パッシブスキル実行後は元に戻す
        doBattleMonsterIndex = beforeDoBattleMonsterIndex;
        beDoneBattleMonsterDataList = beforeBeDoneBattleMonsterDataList;
    }

    private BattlePhase GetNextBattlePhase(BattlePhase currentBattlePhase)
    {
        var enumCount = Enum.GetNames(typeof(BattlePhase)).Length;
        var currentNum = (int)currentBattlePhase;
        var nextNum = currentNum == enumCount - 1 ? 0 : currentNum + 1;
        return (BattlePhase)nextNum;
    }

    private enum BattlePhase
    {
        /// <summary>
        /// 行動するモンスターを決める
        /// </summary>
        SetDoMonster = 0,

        /// <summary>
        /// 必要であれば次のウェーブに進む
        /// </summary>
        MoveNextWave,

        /// <summary>
        /// 必要であれば次のターンに進む
        /// </summary>
        MoveNextTurn,

        /// <summary>
        /// 行動するモンスターが使用するスキルを決める
        /// </summary>
        SetDoSkill,

        /// <summary>
        /// 行動を実行する
        /// </summary>
        ActivateSkill,

        /// <summary>
        /// スキルの効果を反映させる
        /// </summary>
        ReflectSkill,

        /// <summary>
        /// ゲームの勝敗判定を行う
        /// </summary>
        JudgeBattleEnd,
    }

    private enum BattleMonsterActType
    {
        PassiveSkill,
        NormalSkill,
        UltimateSkill,
    }
}

public class BattleMonsterAndSkillEffectListSet
{
    public BattleMonsterInfo battleMonster { get; set; }
    public List<SkillEffectMI> skillEffectList { get; set; }
}
