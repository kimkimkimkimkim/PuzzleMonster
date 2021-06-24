using GameBase;

[ResourcePath("UI/Parts/Parts-BattleBoardPieceItem")]
public class BattleBoardPieceItem : BattlePieceItem
{
    public int index { get; private set; }

    public void SetIndex(int index)
    {
        this.index = index;
    }
}