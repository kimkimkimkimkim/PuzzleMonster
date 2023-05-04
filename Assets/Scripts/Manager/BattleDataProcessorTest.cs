using System.Collections.Generic;
using System.Linq;

public partial class BattleDataProcessor
{
    private bool isTest;
    private List<List<BattleMonsterInfo>> testEnemyBattleMonsterListByWave;

    public void TestInit(int currentWaveCount = 1, int currentTurnCount = 1)
    {
        // BattleDataProcessor�ł�limitTurnNum��questMonsterListByWave�����g�p���Ă��Ȃ�
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

        // �����ݒ�
        Init(new List<UserMonsterInfo>(), quest);

        this.currentWaveCount = currentWaveCount;
        this.currentTurnCount = currentTurnCount;
    }

    /// <summary>
    /// �e�X�g���J�n�����O��Ԃ�
    /// </summary>
    public List<BattleLogInfo> TestStart(List<BattleMonsterInfo> playerBattleMonsterList, List<List<BattleMonsterInfo>> enemyBattleMonsterListByWave, int loopNum = 1)
    {
        // �e�X�g�p�̏����ݒ�
        isTest = true;
        testEnemyBattleMonsterListByWave = enemyBattleMonsterListByWave;

        // �����X�^�[���X�g���e�X�g�p�ɉ��H
        var waveIndex = currentWaveCount - 1;
        this.playerBattleMonsterList = playerBattleMonsterList;
        this.enemyBattleMonsterList = enemyBattleMonsterListByWave[waveIndex];

        // �o�g�������J�n
        for (var i = 0; i < loopNum; i++)
        {
            PlayLoop();
        }

        // ���O���o��
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