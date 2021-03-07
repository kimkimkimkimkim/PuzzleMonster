using System.Collections.Generic;

/// <summary>
/// コマンドデータ
/// </summary>
public class CommandMB
{
    /// <summary>
    /// ID
    /// </summary>
    public long id { get; set; }

    /// <summary>
    /// コマンド成功と判定される方向リスト
    /// </summary>
    public List<Direction> directionList { get; set; }

}
