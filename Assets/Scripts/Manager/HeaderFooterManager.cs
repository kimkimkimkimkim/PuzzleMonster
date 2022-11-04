using GameBase;
using PM.Enum.UI;
using System.Collections.Generic;

public class HeaderFooterManager : SingletonMonoBehaviour<HeaderFooterManager>
{
    private HeaderFooterWindowUIScript uiScript;

    public void SetHeaderFooterWindowUIScript(HeaderFooterWindowUIScript uiScript)
    {
        this.uiScript = uiScript;
    }

    /// <summary>
    /// プロパティパネルの表示を更新
    /// </summary>
    public void UpdatePropertyPanelText()
    {
        if (uiScript == null) return;
        uiScript.UpdatePropertyPanelText();
    }

    /// <summary>
    /// 指定したプロパティパネルを指定した順序(右上から)で表示
    /// </summary>
    public void ShowPropertyPanel(List<PropertyPanelType> propertyPanelTypeList) {
        if (uiScript == null) return;
        uiScript.ShowPropertyPanel(propertyPanelTypeList);
    }

    /// <summary>
    /// スタミナ値を更新
    /// </summary>
    public void SetStaminaText()
    {
        if (uiScript == null) return;
        uiScript.SetStaminaUI();
    }

    public void UpdateUserDataUI()
    {
        if (uiScript == null) return;
        uiScript.UpdateUserDataUI();
    }

    /// <summary>
    /// 表示制御
    /// </summary>
    public void Show(bool isShow){
        if (uiScript == null) return;
        uiScript.headerPanel.SetActive(isShow);
        uiScript.footerPanel.SetActive(isShow);
    }

    public void ActivateBadge()
    {
        if (uiScript != null) uiScript.ActivateBadge();
    }
}

