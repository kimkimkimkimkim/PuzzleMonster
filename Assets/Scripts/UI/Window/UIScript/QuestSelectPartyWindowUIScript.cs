using GameBase;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Collections.Generic;
using System.Linq;
using PM.Enum.UI;

[ResourcePath("UI/Window/Window-QuestSelectParty")]
public class QuestSelectPartyWindowUIScript : WindowBase
{
    [SerializeField] protected Text _titleText;
    [SerializeField] protected Button _okButton;
    [SerializeField] protected Button _grayoutButton;
    [SerializeField] protected GameObject _okButtonGrayoutPanel;
    [SerializeField] protected List<ToggleWithValue> _tabList;
    [SerializeField] protected List<PartyMonsterIconItem> _partyMonsterIconList;
    [SerializeField] protected InfiniteScroll _infiniteScroll;
    [SerializeField] protected ToggleGroup _toggleGroup;

    private GrayoutReason grayoutReason;
    private QuestMB quest;
    private int currentPartyIndex = 0;
    private int selectedPartyMonsterIndex = -1;
    private string selectedUserMonsterId = null;
    private List<UserMonsterInfo> userMonsterList;
    private UserMonsterPartyInfo _currentUserMonsterParty;
    // TODO: �T�[�o�[����null����������ԂŕԂ��Ă���悤�ɂȂ����炱�̃v���p�e�B�͍폜
    private UserMonsterPartyInfo currentUserMonsterParty
    {
        get
        {
            // �p�[�e�B��񂪖�����΃_�~�[�f�[�^���쐬
            if (_currentUserMonsterParty == null)
            {
                _currentUserMonsterParty = new UserMonsterPartyInfo()
                {
                    id = null, 
                    partyIndex = currentPartyIndex,
                    userMonsterIdList = new List<string>(),
                };
            };

            // �p�[�e�B�����o�[���ɒB���Ă��Ȃ���΃_�~�[�f�[�^��ǉ�
            while(_currentUserMonsterParty.userMonsterIdList.Count < ConstManager.Battle.MAX_PARTY_MEMBER_NUM)
            {
                _currentUserMonsterParty.userMonsterIdList.Add(null);
            }
            return _currentUserMonsterParty;
        }
        set
        {
            _currentUserMonsterParty = value;
        }
    }

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        var questId = (long)info.param["questId"];
        quest = MasterRecord.GetMasterOf<QuestMB>().Get(questId);
        currentUserMonsterParty = ApplicationContext.userData.userMonsterPartyList.FirstOrDefault(u => u.partyIndex == currentPartyIndex)?.Clone();
        userMonsterList = ApplicationContext.userData.userMonsterList.OrderBy(u => u.monsterId).ToList();

        // �͂����A�C�R���悤��null�f�[�^��擪�ɒǉ�
        userMonsterList.Insert(0, null);

        _titleText.text = quest.name;

        _okButton.OnClickIntentAsObservable()
            .SelectMany(_ =>
            {
                var userMonsterParty = ApplicationContext.userData.userMonsterPartyList.FirstOrDefault(u => u.id == currentUserMonsterParty.id);
                if(userMonsterParty != null && userMonsterParty.IsSame(currentUserMonsterParty))
                {
                    // ���ݑI�𒆂̃p�[�e�B��񂪑��݂������ύX���Ȃ���΂��̂܂ܐi��
                    return Observable.ReturnUnit();
                }
                else
                {
                    // �ύX���������Ă����ꍇ�̓��[�U�[�f�[�^���X�V����
                    return ApiConnection.UpdateUserMosnterFormation(currentPartyIndex, currentUserMonsterParty.userMonsterIdList)
                        .Do(res => currentUserMonsterParty = res.userMonsterParty)
                        .AsUnitObservable();
                }
            })
            .SelectMany(_ =>
            {
                return BattleManager.Instance.StartBattleObservable(questId, currentUserMonsterParty.id);
            })
            .Subscribe();

        _grayoutButton.OnClickAsObservable()
            .SelectMany(_ =>
            {
                switch (grayoutReason)
                {
                    case GrayoutReason.NotExistsMonster:
                        return CommonDialogFactory.Create(new CommonDialogRequest()
                        {
                            commonDialogType = CommonDialogType.YesOnly,
                            content = "�����X�^�[��1�̈ȏ�I�����Ă�������",
                        }).AsUnitObservable();
                    case GrayoutReason.NotEnoughStamina:
                        return CommonDialogFactory.Create(new CommonDialogRequest()
                        {
                            commonDialogType = CommonDialogType.YesOnly,
                            content = "���킷�邽�߂̃X�^�~�i������܂���",
                        }).AsUnitObservable();
                    case GrayoutReason.NotEnoughMaxStamina:
                        var rank = MasterRecord.GetMasterOf<StaminaMB>().GetAll().FirstOrDefault(m => m.stamina >= quest.consumeStamina)?.rank ?? 0;
                        return CommonDialogFactory.Create(new CommonDialogRequest()
                        {
                            commonDialogType = CommonDialogType.YesOnly,
                            content = $"���̃N�G�X�g�̓����N{rank}�ȏ�Œ��킷�邱�Ƃ��ł��܂�",
                        }).AsUnitObservable();
                    default:
                        return Observable.ReturnUnit();
                }
            })
            .Subscribe();

        SetTabChangeAction();
        RefreshPartyUI();
        RefreshScroll();
        RefreshGrayoutPanel();
    }

    private void SetTabChangeAction()
    {
        _tabList.ForEach(tab =>
        {
            tab.OnValueChangedIntentAsObservable()
                .Where(isOn => isOn)
                .Do(_ =>
                {
                    var partyIndex = tab.value; // �����ł̓^�u�̒l���p�[�e�B�C���f�b�N�X�ɑΉ�����
                    currentPartyIndex = partyIndex;
                    currentUserMonsterParty = ApplicationContext.userData.userMonsterPartyList.FirstOrDefault(u => u.partyIndex == currentPartyIndex)?.Clone();
                    _toggleGroup.SetAllTogglesOff();
                    RefreshPartyUI();
                    RefreshScroll();
                    RefreshGrayoutPanel();
                })
                .Subscribe();
        });
    }

    private void RefreshPartyUI()
    {
        if(currentUserMonsterParty == null)
        {
            _partyMonsterIconList.ForEach(i => i.ShowIconItem(false));
            return;
        }

        _partyMonsterIconList.ForEach((monsterIcon, index) =>
        {
            var isOutOfIndex = index >= currentUserMonsterParty.userMonsterIdList.Count;
            var userMonsterId = isOutOfIndex ? null : currentUserMonsterParty.userMonsterIdList[index];
            var userMonster = userMonsterList.FirstOrDefault(u => u?.id == userMonsterId);

            if (userMonster == null)
            {
                monsterIcon.ShowIconItem(false);
            }
            else
            {
                var itemMI = ItemUtil.GetItemMI(userMonster);
                monsterIcon.ShowIconItem(true);
                monsterIcon.iconItem.SetIcon(itemMI);
            }

            monsterIcon.SetOnClickAction(() =>
            {
                if(selectedPartyMonsterIndex != -1 && selectedPartyMonsterIndex != index && (selectedUserMonsterId != null || userMonsterId != null))
                {
                    // �I�𒆂̃����X�^�[���p�[�e�B�Ґ�����Ă��鎩���ȊO�̃����X�^�[�̏ꍇ
                    currentUserMonsterParty.userMonsterIdList[selectedPartyMonsterIndex] = userMonsterId;
                    currentUserMonsterParty.userMonsterIdList[index] = selectedUserMonsterId;

                    _toggleGroup.SetAllTogglesOff();
                    selectedPartyMonsterIndex = -1;
                    selectedUserMonsterId = null;
                    RefreshPartyUI();
                    RefreshScroll(); // TODO: �����������ɕ\�����̃A�C�e�������X�V������
                    RefreshGrayoutPanel();
                }
                else if(selectedUserMonsterId != null)
                {
                    // �I�𒆂̃����X�^�[���p�[�e�B�ɕҐ�����Ă��Ȃ��ꍇ
                    currentUserMonsterParty.userMonsterIdList[index] = selectedUserMonsterId;

                    _toggleGroup.SetAllTogglesOff();
                    selectedPartyMonsterIndex = -1;
                    selectedUserMonsterId = null;
                    RefreshPartyUI();
                    RefreshScroll(); // TODO: �����������ɕ\�����̃A�C�e�������X�V������
                    RefreshGrayoutPanel();
                }
                else
                {
                    // �I�𒆂̃����X�^�[�����݂��Ȃ����邢�͎����̏ꍇ
                    monsterIcon.toggle.isOn = !monsterIcon.toggle.isOn;
                    if (monsterIcon.toggle.isOn)
                    {
                        selectedPartyMonsterIndex = index;
                        selectedUserMonsterId = userMonsterId;
                    }
                    else
                    {
                        // ��I���ɂ���ꍇ�͑I�𒆃��[�U�[�����X�^�[ID��null�ɁAIndex��-1��
                        selectedPartyMonsterIndex = -1;
                        selectedUserMonsterId = null;
                    }
                }
            });
        });
    }

    private void RefreshScroll()
    {
        _infiniteScroll.Clear();

        _infiniteScroll.Init(userMonsterList.Count, OnUpdateItem);
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((userMonsterList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<IconItem>();
        var userMonster = userMonsterList[index];
        var userMonsterId = userMonster?.id;

        if(userMonster == null)
        {
            // �͂����A�C�R��
            scrollItem.ShowIcon(false);
            scrollItem.ShowText(true, "�͂���");
        }
        else
        {
            // �����X�^�[�A�C�R��
            var itemMI = ItemUtil.GetItemMI(userMonster);
            var isIncludedParty = currentUserMonsterParty.userMonsterIdList.Contains(userMonster.id);

            scrollItem.ShowText(true);
            scrollItem.SetIcon(itemMI);
            scrollItem.ShowText(false);
            scrollItem.ShowGrayoutPanel(isIncludedParty, "�Ґ���");
        }

        scrollItem.SetToggleGroup(_toggleGroup);
        scrollItem.SetOnClickAction(() =>
        { 
            if(selectedPartyMonsterIndex != -1)
            {
                // �Ґ����̃����X�^�[��I��
                currentUserMonsterParty.userMonsterIdList[selectedPartyMonsterIndex] = userMonsterId;

                _toggleGroup.SetAllTogglesOff();
                selectedPartyMonsterIndex = -1;
                selectedUserMonsterId = null;
                RefreshPartyUI();
                RefreshScroll(); // TODO: �����������ɕ\�����̃A�C�e�������X�V������
                RefreshGrayoutPanel();
            }
            else
            {
                // �ʏ�ʂ�I��
                scrollItem.toggle.isOn = !scrollItem.toggle.isOn;
                if (scrollItem.toggle.isOn)
                {
                    selectedPartyMonsterIndex = -1;
                    selectedUserMonsterId = userMonsterId;
                }
                else
                {
                    // ��I���ɂ���ꍇ�͑I�𒆃��[�U�[�����X�^�[ID��null�ɁAIndex��-1��
                    selectedPartyMonsterIndex = -1;
                    selectedUserMonsterId = null;
                }
            }
        });
    }

    private void RefreshGrayoutPanel()
    {
        var existsMonster = currentUserMonsterParty.userMonsterIdList.Any(id => id != null);
        var enoughStamina = ApplicationContext.userData.stamina >= quest.consumeStamina;
        var rank = MasterRecord.GetMasterOf<StaminaMB>().GetAll().FirstOrDefault(m => m.rank == ApplicationContext.userData.rank);
        var maxStamina = rank?.stamina ?? 0;
        var enoughMaxStamina = maxStamina >= quest.consumeStamina;

        grayoutReason = 
            !existsMonster ? GrayoutReason.NotExistsMonster
            : !enoughStamina ? GrayoutReason.NotEnoughStamina
            : !enoughMaxStamina ? GrayoutReason.NotEnoughMaxStamina
            : GrayoutReason.None;

        _okButtonGrayoutPanel.SetActive(grayoutReason != GrayoutReason.None);
    }

    public override void Open(WindowInfo info)
    {
    }

    public override void Back(WindowInfo info)
    {
    }

    public override void Close(WindowInfo info)
    {
        base.Close(info);
    }

    private enum GrayoutReason
    {
        None,
        NotExistsMonster,
        NotEnoughStamina,
        NotEnoughMaxStamina,
    }
}
