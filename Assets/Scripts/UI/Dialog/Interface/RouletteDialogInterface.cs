using System.Collections.Generic;

public class RouletteDialogRequest
{
    /// <summary>
    /// アイテムリスト
    /// 要素数は8
    /// </summary>
    public List<ItemMI> itemList;

    /// <summary>
    /// 当選アイテムインデックス
    /// </summary>
    public int electedIndex;
}

public class RouletteDialogResponse
{
}