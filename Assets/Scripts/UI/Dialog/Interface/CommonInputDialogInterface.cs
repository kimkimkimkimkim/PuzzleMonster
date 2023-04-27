using PM.Enum.UI;

public class CommonInputDialogRequest
{
    /// <summary>
    /// 本文
    /// </summary>
    public string contentText { get; set; }
}

public class CommonInputDialogResponse
{
    /// <summary>
    /// ダイアログレスポンスタイプ
    /// </summary>
    public DialogResponseType dialogResponseType { get; set; }

    /// <summary>
    /// 入力された文字列
    /// </summary>
    public string inputText { get; set; }
}