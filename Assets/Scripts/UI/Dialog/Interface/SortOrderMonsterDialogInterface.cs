using PM.Enum.Monster;
using PM.Enum.SortOrder;
using PM.Enum.UI;
using System.Collections.Generic;

public class SortOrderMonsterDialogRequest
{
    /// <summary>
    /// �����i�荞�݂̏����l
    /// </summary>
    public List<MonsterAttribute> initialFilterAttribute { get; set; }

    /// <summary>
    /// ���я��̏����l
    /// </summary>
    public SortOrderTypeMonster initialSortOrderType { get; set; }
}

public class SortOrderMonsterDialogResponse
{
    /// <summary>
    /// �_�C�A���O���X�|���X���
    /// </summary>
    public DialogResponseType dialogResponseType { get; set; }

    /// <summary>
    /// �����i�荞��
    /// </summary>
    public List<MonsterAttribute> filterAttribute { get; set; }

    /// <summary>
    /// ���я�
    /// </summary>
    public SortOrderTypeMonster sortOrderType { get; set; }
}