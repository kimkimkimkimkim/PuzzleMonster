public class MonsterLuckUpDialogRequest
{
    /// <summary>
    /// ���[�U�[�����X�^�[���
    /// </summary>
    public UserMonsterInfo userMonster { get; set; }
}

public class MonsterLuckUpDialogResponse
{
    /// <summary>
    /// �X�V���K�v���ۂ�
    /// </summary>
    public bool isNeedRefresh { get; set; }
}