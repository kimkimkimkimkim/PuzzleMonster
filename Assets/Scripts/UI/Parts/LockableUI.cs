using GameBase;
using PM.Enum.UI;
using UnityEngine;

public class LockableUI : MonoBehaviour
{
    [SerializeField] protected long _lockableId;
    [SerializeField] protected LockType _lockType;
    [SerializeField] protected GameObject _targetObject;

    public LockableMB lockable { get; private set; }

    private void Start()
    {
        var lockable = MasterRecord.GetMasterOf<LockableMB>().Get(_lockableId);
        var shouldLock = ConditionUtil.IsValid(ApplicationContext.userData, lockable.lockConditionList);

        // 現状アンロックからロックになることはないのでそもそもアンロックのものはリストに追加しない
        if (shouldLock) {
            this.lockable = lockable;
            UIManager.Instance.AddLockableUI(this);
        }
    }

    public void RefreshUI(bool isLock) {
        switch (_lockType) {
            case LockType.ShowBlocker:
                // ロック状態なら表示する
                _targetObject.SetActive(isLock);
                break;
            case LockType.MakeInvisible:
                // ロック状態なら非表示にする
                _targetObject.SetActive(!isLock);
                break;
            case LockType.None:
            default:
                break;
        }
    }

    private void OnDestroy() {
        UIManager.Instance.RemoveLockableUI(_lockableId);
    }
}
