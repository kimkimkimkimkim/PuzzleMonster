public class PlayerRankUpDialogRequest
{
    /// <summary>
    /// �����N�A�b�v�O�����N
    /// </summary>
    public int beforeRank { get; set; }

    /// <summary>
    /// �����N�A�b�v�ド���N
    /// </summary>
    public int afterRank { get; set; }

    /// <summary>
    /// �����N�A�b�v�O�X�^�~�i�ő�l
    /// </summary>
    public int beforeMaxStamina { get; set; }

    /// <summary>
    /// �����N�A�b�v��X�^�~�i�ő�l
    /// </summary>
    public int afterMaxStamina { get; set; }
}

public class PlayerRankUpDialogResponse
{
}