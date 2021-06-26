using GameBase;

[ResourcePath("UI/Parts/Parts-BattleBoardPieceItem")]
public class BattleBoardPieceItem : BattlePieceItem
{
    public BoardIndex boardIndex { get; private set; }

    public void SetBoardIndex(BoardIndex boardIndex)
    {
        this.boardIndex = boardIndex;
    }
}