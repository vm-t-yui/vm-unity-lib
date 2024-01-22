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
    public static void SetTerm(TextMeshProUGUI ugui, Localize localize, string term, string debugText, bool allowSpaceEmpty = true)
    {
        // Termがなかったら直接文字列設定
        if (LocalizationManager.GetTermsList().Contains(term) == false)
        {
            // デバッグテキストがなしなら直接設定許可と判定
            // allowSpaceがフラグ経っていたら空白文字を許容する
            if (string.IsNullOrEmpty(debugText) || (allowSpaceEmpty && (string.IsNullOrWhiteSpace(term)) || string.IsNullOrEmpty(term)))
            {
                ugui.text = term;
            }
            else
            {
                ugui.text = debugText + term;
            }
        }
        else
        {
            localize.SetTerm(term);
        }
    }
}
