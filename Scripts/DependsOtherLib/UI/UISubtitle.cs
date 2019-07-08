/******************************************************************************/
/*!    \brief  字幕.
*******************************************************************************/

using UnityEngine;
using System;
using VMUnityLib;
using TMPro;
using I2.Loc;

public sealed class UISubtitle : SingletonMonoBehaviour<UISubtitle>
{
    float showedTime;
    float showTime;
    TextMeshProUGUI uiText;
    bool started = false;
    Action onEndPlayVoice;
    Localize localize;
    TextMeshFader fadeColor = new TextMeshFader();

    /// <summary>
    /// 初期化.
    /// </summary>
    private void Start()
    {
        uiText = GetComponent<TextMeshProUGUI>();
        localize   = GetComponent<Localize>();
    }
    
    /// <summary>
    /// 更新.
    /// </summary>
    private void Update()
    {
        fadeColor.UpdateFade();
        if (started && Time.unscaledTime - showedTime > showTime)
        {
            started = false;
            fadeColor.FadeOut(uiText, fadeColor.FadeSpeedBase);
            if(onEndPlayVoice != null)
            {
                onEndPlayVoice();
            }
        }
    }

    /// <summary>
    /// サブタイトル表示開始.
    /// </summary>
    public void SetSubtiltle(string str, float time, Action in_onEndPlayVoice)
    {
        showTime = time;
        showedTime = Time.unscaledTime;
        onEndPlayVoice = in_onEndPlayVoice;
        localize.Term = "subTitle/" + str;
        if (!started)
        {
            fadeColor.FadeIn(uiText, fadeColor.FadeSpeedBase);
        }
        started = true;
    }
}
