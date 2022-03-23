using PM.Enum.Battle;
using System.Collections.Generic;

public class BattleResultDialogRequest
{
    /// <summary>
    /// ���s
    /// </summary>
    public WinOrLose winOrLose { get; set; }

    /// <summary>
    /// �X�R�A�p�̖����o�g�������X�^�[���X�g
    /// </summary>
    public List<BattleMonsterInfo> playerBattleMonsterList { get; set; }

    /// <summary>
    /// �X�R�A�p��Wave���G�o�g�������X�^�[���X�g
    /// </summary>
    public List<List<BattleMonsterInfo>> enemyBattleMonsterListByWave { get; set; }
}

public class BattleResultDialogResponse
{
}