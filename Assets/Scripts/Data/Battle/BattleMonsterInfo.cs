using System.Collections.Generic;

public class BattleMonsterInfo
{
    /// <summary>
    /// レベル
    /// </summary>
    public int level{ get; set; }
     
    /// <summary>
    /// 最大体力
    /// </summary>
    public int maxHp{ get; set; }
     
    /// <summary>
    /// 現在の体力
    /// </summary>
    public int currentHp{ get; set; }

    /// <summary>
    /// 基準攻撃力
    /// </summary>
    public int baseAttack{ get; set; }
     
    /// <summary>
    /// 現在の攻撃力
    /// </summary>
    public float currentAttack{ get; set; }

    /// <summary>
    /// 基準防御力
    /// </summary>
    public int baseDefense{ get; set; }

    /// <summary>
    /// 現在の防御力
    /// </summary>
    public float currentDefense{ get; set; }

    /// <summary>
    /// 基準スピード
    /// </summary>
    public int baseSpeed{ get; set; }
     
    /// <summary>
    /// 現在のスピード
    /// </summary>
    public float currentSpeed{ get; set; }
     
    /// <summary>
    /// 基準回復力
    /// </summary>
    public int baseHeal{ get; set; }
     
    /// <summary>
    /// 現在の回復力
    /// </summary>
    public float currentHeal{ get; set; }
    
    /// <summary>
    /// 最大のクールタイム
    /// </summary>
    public float maxCt{ get; set; }
     
    /// <summary>
    /// 現在のクールタイム
    /// </summary>
    public float currentCt{ get; set; }
     
    /// <summary>
    /// 状態異常リスト
    /// </summary>
    public List<BattleConditionInfo> battleConditionList{ get; set; }
    
    /// <summary>
    /// このターンすでに行動しているか否か
    /// </summary>
    public bool isActed{ get; set; }

    /// <summary>
    /// バトルモンスターインデックス
    /// </summary>
    public BattleMonsterIndex index{ get; set; }
}
