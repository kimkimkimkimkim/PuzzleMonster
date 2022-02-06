using GameBase;
using UniRx;

public class HeaderFooterManager : SingletonMonoBehaviour<HeaderFooterManager>
{
    private HeaderFooterWindowUIScript uiScript;

    public void SetHeaderFooterWindowUIScript(HeaderFooterWindowUIScript uiScript)
    {
        this.uiScript = uiScript;
    }

    /// <summary>
    /// 仮想通貨の所持数を更新
    /// </summary>
    public void UpdateVirutalCurrencyText()
    {
        if (uiScript == null) return;
        uiScript.UpdateVirtualCurrencyText();
    }

    /// <summary>
    /// スタミナ値を更新
    /// </summary>
    public void SetStaminaText()
    {
        if (uiScript == null) return;
        uiScript.SetStaminaText();
    }

    /// <summary>
    /// 表示制御
    /// </summary>
    public void Show(bool isShow){
        if (uiScript == null) return;
        uiScript.headerPanel.SetActive(isShow);
        uiScript.footerPanel.SetActive(isShow);
    }
}

