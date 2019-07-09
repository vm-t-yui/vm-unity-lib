/******************************************************************************/
/*!    \brief  ローカライズ用テキスト設定.
*******************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class TextLocalizer : MonoBehaviour
{
    [Multiline]
    public string englishText;
    public int englishFontSize;
    public Font font;
    public FontStyle fontStyle = FontStyle.Normal;
    public Vector3 posAdjust = Vector3.zero;

    Vector3 defaultPos;
    bool bFirst = true;
    Text text;

    void Start()
    {
        UpdateText();
    }

    public void UpdateText()
    {
        if (bFirst)
        {
            text = GetComponent<Text>();
            defaultPos = transform.position;
            bFirst = false;
        }
        if (Application.systemLanguage != SystemLanguage.Japanese)
        {
            if (englishText != string.Empty)
            {
                text.text = englishText;
            }
            text.font = font;
            text.fontStyle = fontStyle;
            transform.position = defaultPos + posAdjust;
            if (englishFontSize != 0)
            {
                text.fontSize = englishFontSize;
            }
        }
    }
}
