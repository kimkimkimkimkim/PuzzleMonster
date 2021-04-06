using System.Collections.Generic;

public class ExecuteGachaApiRequest
{
    /// <summary>
    /// ガチャボックス詳細ID
    /// </summary>
    public long gachaBoxDetailId { get; set; }
}

public class ExecuteGachaApiResponse
{
    /// <summary>
    /// 処理が成功したか否か
    /// </summary>
    public bool isSuccess { get; set; }

    /// <summary>
    /// 報酬アイテムリスト
    /// </summary>
    public List<ItemMI> rewardItemList { get; set; } = new List<ItemMI>();
}
