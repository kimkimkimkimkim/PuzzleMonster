using PM.Enum.UI;

public class CommonInputDialogRequest
{
    /// <summary>
    /// �{��
    /// </summary>
    public string contentText { get; set; }
}

public class CommonInputDialogResponse
{
    /// <summary>
    /// �_�C�A���O���X�|���X�^�C�v
    /// </summary>
    public DialogResponseType dialogResponseType { get; set; }

    /// <summary>
    /// ���͂��ꂽ������
    /// </summary>
    public string inputText { get; set; }
}