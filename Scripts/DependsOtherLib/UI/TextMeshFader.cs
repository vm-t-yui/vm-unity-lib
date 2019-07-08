/******************************************************************************/
/*!    \brief  TextMeshProのフェードを介した表示非表示の切り替え.
*******************************************************************************/
using UnityEngine;
using TMPro;

public class TextMeshFader
{
    TextMeshProUGUI text = null;
    public float FadeSpeedBase { get { return 0.1f; } }
    float dir   = 0;
    float speed = 0.0f;
    /// <summary>
    /// 更新
    /// </summary>
    public void UpdateFade()
    {
        // dirの役割
        // dir =  1 = フェードイン
        //       -1 = フェードアウト
        //        0 = 待機 
        if (dir  == 0.0f)
        {
            return;
        }
        // アルファ値制御
        text.color = new Color(text.color.r,
                               text.color.g,
                               text.color.b,
                               text.color.a + dir * speed);
        // 以下終了処理
        if (text.color.a <= 0.0f)
        {
            dir = 0.0f;
            text.color = new Color(text.color.r, text.color.g, text.color.b, 0.0f);
            text.gameObject.SetActive(false);
        }
        if (text.color.a >= 1.0f)
        {
            dir = 0.0f;
            text.color = new Color(text.color.r, text.color.g, text.color.b, 1.0f);
        }
    }
    /// <summary>
    /// 徐々に表示へ(透明度は0.0f～1.0f)
    /// </summary>
    public void FadeIn(TextMeshProUGUI inText,  float inFadeSpeed)
    {
        text  = inText;
        dir   = 1;
        speed = inFadeSpeed;
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0.0f);
        text.gameObject.SetActive(true);
    }

    /// <summary>
    /// 徐々に非表示へ(透明度は0.0f～1.0f)
    /// </summary>
    public void FadeOut(TextMeshProUGUI inText, float inFadeSpeed)
    {
        text  = inText;
        dir   = -1;
        speed = inFadeSpeed;
        text.color = new Color(text.color.r, text.color.g, text.color.b, 1.0f);
        text.gameObject.SetActive(true);
    }
}
