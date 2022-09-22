using GameBase;

public class MainSceneManager : SingletonMonoBehaviour<MainSceneManager>
{
    private bool isReadyToShowStackableDialog = false;

    public void SetIsReadyToShowStackableDialog(bool isReady)
    {
        isReadyToShowStackableDialog = isReady;
    }

    private void Update() {
        if (isReadyToShowStackableDialog) {
            if (UIManager.Instance.currentWindowInfo?.component is HomeWindowUIScript && UIManager.Instance.currentDialogInfo == null) {
                StackableDialogManager.Instance.Call();
            }
        }
    }
}
