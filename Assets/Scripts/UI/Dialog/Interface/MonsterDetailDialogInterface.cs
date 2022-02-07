public class MonsterDetailDialogRequest
{
    /// <summary>
    /// 表示するモンスターのユーザーモンスター情報
    /// </summary>
    public UserMonsterInfo userMonster { get; set; }
}

public class MonsterDetailDialogResponse
{
    /// <summary>
    /// 更新が必要か否か
    /// </summary>
    public bool isNeedRefresh { get; set; }
}