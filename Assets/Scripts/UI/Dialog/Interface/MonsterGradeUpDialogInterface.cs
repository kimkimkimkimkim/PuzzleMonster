public class MonsterGradeUpDialogRequest
{
    /// <summary>
    /// ユーザーモンスター情報
    /// </summary>
    public UserMonsterInfo userMonster { get; set; }
}

public class MonsterGradeUpDialogResponse
{
    /// <summary>
    /// 更新が必要か否か
    /// </summary>
    public bool isNeedRefresh { get; set; }
}