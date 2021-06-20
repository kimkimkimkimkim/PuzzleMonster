using System.Collections.Generic;
using System.ComponentModel;

/// <summary>
/// ピースマスタ
/// </summary>
[Description("PieceMB")]
public class PieceMB : MasterBookBase
{
    /// <summary>
    /// 名前
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 横軸か否か
    /// </summary>
    public bool isHorizontal { get; set; }

    /// <summary>
    /// 横軸方向の個数制限（0は制限なし）
    /// </summary>
    public int horizontalConstraint { get; set; }

    /// <summary>
    /// 縦軸方向の個数制限（0は制限なし）
    /// </summary>
    public int verticalConstraint { get; set; }

    /// <summary>
    /// 生成するピースのリスト
    /// trueは実際のピース
    /// falseは透明のピース
    /// </summary>
    public List<bool> pieceList { get; set; }
}
