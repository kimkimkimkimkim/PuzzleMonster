using System.ComponentModel;

/// <summary>
/// マスタデータの基本項目を定義するクラス
/// </summary>
public abstract class MasterBookBase
{
    /// <summary>
    /// ID
    /// </summary>
    [Description("id")]
    public long id { get; set; }
}