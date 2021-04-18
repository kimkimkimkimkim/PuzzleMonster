using System.Collections.Generic;
using System.ComponentModel;
using PM.Enum.Gacha;
using PM.Enum.Monster;

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
    /// 属性
    /// </summary>
    public MonsterAttribute attribute { get; set; }

    /// <summary>
    /// 初期グレード
    /// </summary>
    public int initialGrade { get; set; }

    /// <summary>
    /// ガチャボックスタイプリスト
    /// </summary>
    public List<GachaBoxType> gachaBoxTypeList { get; set; }

    /// <summary>
    /// レベル1でのHP
    /// </summary>
    public int level1Hp { get; set; }

    /// <summary>
    /// レベル1での攻撃力
    /// </summary>
    public int level1Attack { get; set; }

    /// <summary>
    /// レベル1での回復力
    /// </summary>
    public int level1Heal { get; set; }

    /// <summary>
    /// レベル100でのHP
    /// </summary>
    public int level100Hp { get; set; }

    /// <summary>
    /// レベル100での攻撃力
    /// </summary>
    public int level100Attack { get; set; }

    /// <summary>
    /// レベル100での回復力
    /// </summary>
    public int level100Heal { get; set; }
}
