using GameBase;
using PM.Enum.UI;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-BattleMonsterBase")]
public class BattleMonsterBase : MonoBehaviour
{
    public BattleMonsterItem battleMonsterItem { get; private set; }

    public void SetBattleMonsterItem(BattleMonsterItem battleMonsterItem)
    {
        this.battleMonsterItem = battleMonsterItem;
    }
}