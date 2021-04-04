using System.Collections.Generic;
using System.ComponentModel;
using PM.Enum.Gacha;

/// <summary>
/// モンスターマスターデータ
/// </summary>
[Description("MonsterMB")]
public class MonsterMB : MasterBookBase
{
    /// <summary>
    /// 名前
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// 初期グレード
    /// </summary>
    public int initialGrade { get; set; }

    /// <summary>
    /// ガチャボックスタイプリスト
    /// </summary>
    public List<GachaBoxType> gachaBoxTypeList { get; set; }
}
