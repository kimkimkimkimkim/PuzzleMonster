using System.ComponentModel;

[Description("GachaBoxMB")]
public class GachaBoxMB : MasterBookBase
{
    /// <summary>
    /// ガチャ筐体名
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// ドロップテーブル名
    /// </summary>
    public string dropTableName { get; set; }
}
