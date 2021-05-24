using System;
using System.Collections.Generic;

/// <summary>
/// ユーザーデータ情報
/// </summary>
public class UserDataInfo
{
    /// <summary>
    /// ユーザーモンスターパーティーリスト
    /// </summary>
    public List<UserMonsterPartyInfo> userMonsterPartyList { get; set; }

    /// <summary>
    /// 前回のログイン日時
    /// </summary>
    public DateTime lastLoginDateTime { get; set; }

    /// <summary>
    /// 前回スタミナ計算を行った日時
    /// </summary>
    public DateTime lastCalculatedStaminaDateTime { get; set; }

    /// <summary>
    /// スタミナ
    /// </summary>
    public int stamina { get; set; }
}
