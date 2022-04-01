public class MonsterLuckUpDialogRequest
{
    /// <summary>
    /// ユーザーモンスター情報
    /// </summary>
    public UserMonsterInfo userMonster { get; set; }
}

public class MonsterLuckUpDialogResponse
{
    /// <summary>
    /// 更新が必要か否か
    /// </summary>
    public bool isNeedRefresh { get; set; }
}