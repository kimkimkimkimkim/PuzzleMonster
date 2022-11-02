using PM.Enum.Monster;
using PM.Enum.SortOrder;
using PM.Enum.UI;
using System.Collections.Generic;

public class SortOrderMonsterDialogRequest
{
    /// <summary>
    /// 属性絞り込みの初期値
    /// </summary>
    public List<MonsterAttribute> initialFilterAttribute { get; set; }

    /// <summary>
    /// 並び順の初期値
    /// </summary>
    public SortOrderTypeMonster initialSortOrderType { get; set; }
}

public class SortOrderMonsterDialogResponse
{
    /// <summary>
    /// ダイアログレスポンス種類
    /// </summary>
    public DialogResponseType dialogResponseType { get; set; }

    /// <summary>
    /// 属性絞り込み
    /// </summary>
    public List<MonsterAttribute> filterAttribute { get; set; }

    /// <summary>
    /// 並び順
    /// </summary>
    public SortOrderTypeMonster sortOrderType { get; set; }
}