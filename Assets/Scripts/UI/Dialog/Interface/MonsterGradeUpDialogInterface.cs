public class MonsterGradeUpDialogRequest
{
    /// <summary>
    /// ���[�U�[�����X�^�[���
    /// </summary>
    public UserMonsterInfo userMonster { get; set; }
}

public class MonsterGradeUpDialogResponse
{
    /// <summary>
    /// �X�V���K�v���ۂ�
    /// </summary>
    public bool isNeedRefresh { get; set; }
}