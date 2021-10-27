public class BattleMonsterIndex {
    /// <summary>
    /// プレイヤーのモンスターか否か
    /// </summary>
    public bool isPlayer { get; set; }
    
    /// <summary>
    /// インデックス
    /// </summary>
    public int index { get; set; }

    public BattleMonsterIndex() { }

    public BattleMonsterIndex(BattleMonsterIndex battleMonsterIndex)
    {
        this.isPlayer = battleMonsterIndex.isPlayer;
        this.index = battleMonsterIndex.index;
    }
}
