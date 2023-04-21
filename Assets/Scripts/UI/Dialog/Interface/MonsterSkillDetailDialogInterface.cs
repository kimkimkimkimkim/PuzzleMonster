using PM.Enum.Battle;

public class MonsterSkillDetailDialogRequest {
    /// <summary>
    /// ユーザーモンスター
    /// </summary>
    public UserMonsterInfo userMonster { get; set; }

    /// <summary>
    /// バトルアクションタイプ
    /// </summary>
    public BattleActionType battleActionType { get; set; }
}

public class MonsterSkillDetailDialogResponse {
}
