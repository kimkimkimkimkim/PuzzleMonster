using System.Collections.Generic;

public class CommonReceiveDialogRequest {
    /// <summary>
    /// �^�C�g��
    /// </summary>
    public string title { get; set; } = "�A�C�e���󂯎��";

    /// <summary>
    /// �{��
    /// </summary>
    public string content { get; set; } = "�ȉ��̃A�C�e�����󂯎��܂���";

    /// <summary>
    /// �A�C�e�����X�g
    /// </summary>
    public List<ItemMI> itemList { get; set; }
}

public class CommonReceiveDialogResponse {
}
