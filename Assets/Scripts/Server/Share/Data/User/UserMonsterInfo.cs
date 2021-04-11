using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ユーザーのモンスター情報
/// </summary>
public class UserMonsterInfo
{
    /// <summary>
    /// ID
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// モンスターID
    /// </summary>
    public long monsterId { get; set; }

    /// <summary>
    /// レベル
    /// </summary>
    public int level { get; set; }

    /// <summary>
    /// 今まで取得した総経験値
    /// </summary>
    public int exp { get; set; }

    /// <summary>
    /// グレード
    /// </summary>
    public int grade { get; set; }

    /// <summary>
    /// 体力
    /// </summary>
    public int hp { get; set; }

    /// <summary>
    /// 攻撃力
    /// </summary>
    public int attack { get; set; }
}
