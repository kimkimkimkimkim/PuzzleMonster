using GameBase;
using UniRx;

public class HeaderManager : SingletonMonoBehaviour<HeaderManager>
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

    public void SetStaminaText()
    {
        if (uiScript == null) return;
        uiScript.SetStaminaText();
    }
}

