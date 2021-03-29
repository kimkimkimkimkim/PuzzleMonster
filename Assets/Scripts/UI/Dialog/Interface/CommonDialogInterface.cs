using PM.Enum.UI;

public class CommonDialogRequest
{
    /// <summary>
    /// コモンダイアログタイプ
    /// </summary>
    public CommonDialogType commonDialogType { get; set; }

    /// <summary>
    /// タイトル
    /// </summary>
    public string title { get; set; } = "お知らせ";

    /// <summary>
    /// 本文
    /// </summary>
    public string content { get; set; }
}

public class CommonDialogResponse
{
    /// <summary>
    /// ダイアログレスポンス種類
    /// </summary>
    public DialogResponseType dialogResponseType { get; set; }
}