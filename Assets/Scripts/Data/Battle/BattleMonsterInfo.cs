using System.Collections.Generic;

public class BattleMonsterInfo
{
    /// <summary>
    /// レベル
    /// </summary>
    public int level;
     
    /// <summary>
    /// 最大体力
    /// </summary>
    public int maxHp;
     
    /// <summary>
    /// 現在の体力
    /// </summary>
    public int currentHp;

    /// <summary>
    /// 基準攻撃力
    /// </summary>
    public int baseAttack;
     
    /// <summary>
    /// 現在の攻撃力
    /// </summary>
    public float currentAttack;

    /// <summary>
    /// 基準防御力
    /// </summary>
    public int baseDefense;

    /// <summary>
    /// 現在の防御力
    /// </summary>
    public float currentDefense;

    /// <summary>
    /// 基準スピード
    /// </summary>
    public int baseSpeed;
     
    /// <summary>
    /// 現在のスピード
    /// </summary>
    public float currentSpeed;
     
    /// <summary>
    /// 基準回復力
    /// </summary>
    public int baseHeal;
     
    /// <summary>
    /// 現在の回復力
    /// </summary>
    public float currentHeal;
    
    /// <summary>
    /// 最大のクールタイム
    /// </summary>
    public float maxCt;
     
    /// <summary>
    /// 現在のクールタイム
    /// </summary>
    public float currentCt;
     
    /// <summary>
    /// 状態異常リスト
    /// </summary>
    public List<BattleConditionInfo> battleConditionList;
    
    /// <summary>
    /// このターンすでに行動しているか否か
    /// </summary>
    public bool isActed;
}
