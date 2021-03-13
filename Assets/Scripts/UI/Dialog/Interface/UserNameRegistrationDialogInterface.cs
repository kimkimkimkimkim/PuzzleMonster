using Enum.UI;

public class UserNameRegistrationDialogRequest
{

}

public class UserNameRegistrationDialogResponse
{
    /// <summary>
    /// ダイアログレスポンス種類
    /// </summary>
    public DialogResponseType dialogResponseType { get; set; }

    /// <summary>
    /// ユーザー名
    /// </summary>
    public string userName { get; set; }
}

