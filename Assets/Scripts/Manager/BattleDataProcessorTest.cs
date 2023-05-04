using System.Collections.Generic;
using System.Linq;

public partial class BattleDataProcessor
{
    private bool isTest;
    private List<List<BattleMonsterInfo>> testEnemyBattleMonsterListByWave;

    public void TestInit(int currentWaveCount = 1, int currentTurnCount = 1)
    {
        // BattleDataProcessorではlimitTurnNumとquestMonsterListByWaveしか使用していない
        quest = new QuestMB()
        {
            limitTurnNum = 99,
            questMonsterListByWave = new List<List<QuestMonsterMI>>() {
                new List<QuestMonsterMI>() {
                    new QuestMonsterMI() {
                    },
                },
            },
        };

        // 初期設定
        Init(new List<UserMonsterInfo>(), quest);

        this.currentWaveCount = currentWaveCount;
        this.currentTurnCount = currentTurnCount;
    }

    /// <summary>
    /// テストを開始しログを返す
    /// </summary>
    public List<BattleLogInfo> TestStart(List<BattleMonsterInfo> playerBattleMonsterList, List<List<BattleMonsterInfo>> enemyBattleMonsterListByWave, int loopNum = 1)
    {
        // テスト用の初期設定
        isTest = true;
        testEnemyBattleMonsterListByWave = enemyBattleMonsterListByWave;

        // モンスターリストをテスト用に加工
        var waveIndex = currentWaveCount - 1;
        this.playerBattleMonsterList = playerBattleMonsterList;
        this.enemyBattleMonsterList = enemyBattleMonsterListByWave[waveIndex];

        // バトル処理開始
        for (var i = 0; i < loopNum; i++)
        {
            PlayLoop();
        }

        // ログを出力
        return battleLogList;
    }

    public BattleMonsterInfo TestGetBattleMonster(
        long monsterId,
        int monsterLevel,
        bool isPlayer,
        int index,
        int waveCount = 0,
        int currentEnergy = 0,
        List<BattleConditionInfo> battleConditionList = null,
        bool isActed = false,
        bool isDead = false,
        int currentHp = -1,
        int baseSpeed = -1,
        int baseAttack = -1
    )
    {
        var monster = monsterList.First(m => m.id == monsterId);
        var normalSkill = GetBattleMonsterNormalSkill(monster.id, monsterLevel);
        var ultimateSkill = GetBattleMonsterUltimateSkill(monster.id, monsterLevel);
        var passiveSkill = GetBattleMonsterPassiveSkill(monster.id, monsterLevel);
        var battleMonster = BattleUtil.GetBattleMonster(monster, monsterLevel, isPlayer, index, normalSkill, ultimateSkill, passiveSkill, waveCount);

        battleMonster.currentEnergy = currentEnergy;
        if (battleConditionList != null) battleMonster.battleConditionList = battleConditionList;
        battleMonster.isActed = isActed;
        battleMonster.isDead = isDead;
        if (currentHp >= 0) battleMonster.currentHp = currentHp;
        if (baseSpeed >= 0) battleMonster.baseSpeed = baseSpeed;
        if (baseAttack >= 0) battleMonster.baseAttack = baseAttack;

        return battleMonster;
    }

    private void TestSetEnemyBattleMonsterList(int waveCount)
    {
        var waveIndex = waveCount - 1;
        enemyBattleMonsterList = testEnemyBattleMonsterListByWave[waveIndex];
    }
}