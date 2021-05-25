using System.ComponentModel;

/// <summary>
/// スタミナマスタ
/// </summary>
[Description("StaminaMB")]
public class StaminaMB : MasterBookBase
{
    /// <summary>
    /// ランク
    /// </summary>
    public int rank { get; set; }

    /// <summary>
    /// スタミナ
    /// </summary>
    public int stamina { get; set; }
}
