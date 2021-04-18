// モンスター関係のUtil
public static class MonsterUtil
{
    // マスタとレベルからステータスを計算し返す
    // 途中式はfloatで行い最後にintにキャスト
    public static MonsterStatus GetMonsterStatus(MonsterMB monster,int level){
        var hpSlope = (float)(monster.level100Hp - monster.level1Hp)/(100 - 1);
        var hpIntercept = monster.level1Hp - hpSlope;
        var hp = (int)(hpSlope * level + hpIntercept);

        var attackSlope = (float)(monster.level100Attack - monster.level1Attack)/(100 - 1);
        var attackIntercept = monster.level1Attack - attackSlope;
        var attack = (int)(attackSlope * level + attackIntercept);

        var healSlope = (float)(monster.level100Heal - monster.level1Heal)/(100 - 1);
        var healIntercept = monster.level1Heal - healSlope;
        var heal = (int)(healSlope * level + healIntercept);

        return new MonsterStatus (){ hp = hp, attack = attack, heal = heal};
    }

}

// モンスターステータス
public class MonsterStatus {
    public int hp { get; set; }
    public int attack { get; set; }
    public int heal { get; set; }
}