using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandData
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
