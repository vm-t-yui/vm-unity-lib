using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using I2.Loc;
using TMPro;

/// <summary>
/// ローカライズの便利機能
/// </summary>
public static class LocalizeUtility
{
    /// <summary>
    /// 未ローカライズのTerm直接入力に対応したSetTerm
    /// </summary>
    public static void SetTerm(TextMeshProUGUI ugui, Localize localize, string term, string debugText)
    {
        // Termがなかったら直接文字列設定
        if (LocalizationManager.GetTermsList().Contains(term) == false)
        {
#if UNITY_EDITOR // エディタではエディタ用のデバッグテキストつける
            ugui.text = debugText + term;
#else
            ugui.text = player.TargetInteractArea.InteractUITerm;
#endif
        }
        else
        {
            localize.SetTerm(term);
        }
    }
}
