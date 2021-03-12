/// <summary>
/// ドロップタイプ
/// </summary>
public enum DropType
{
    /// <summary>
    /// 通常
    /// </summary>
    Normal = 0,

    /// <summary>
    /// お邪魔
    /// </summary>
    Disturb = 1,
}

/// <summary>
/// コマンドの方向
/// </summary>
public enum Direction
{
    Up,
    Down,
    UpperRight,
    LowerRight,
    UpperLeft,
    LowerLeft,
}

/// <summary>
/// 勝敗判定
/// </summary>
public enum WinOrLose
{
    None,
    Win,
    Lose,
}