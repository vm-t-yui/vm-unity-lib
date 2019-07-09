/******************************************************************************/
/*!    \brief  字幕.
*******************************************************************************/

using UnityEngine;
using System;
using VMUnityLib;
using TMPro;
#if USE_I2_LOCALIZATION
using I2.Loc;
#endif

namespace VMUnityLib
{
    public sealed class UISubtitle : SingletonMonoBehaviour<UISubtitle>
    {
        float showedTime;
        float showTime;
        TextMeshProUGUI uiText;
        bool started = false;
        Action onEndPlayVoice;
#if USE_I2_LOCALIZATION
    Localize localize;
#endif
        TextMeshFader fadeColor = new TextMeshFader();

        /// <summary>
        /// 初期化.
        /// </summary>
        private void Start()
        {
            uiText = GetComponent<TextMeshProUGUI>();
#if USE_I2_LOCALIZATION
        localize   = GetComponent<Localize>();
#endif
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
                if (onEndPlayVoice != null)
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
#if USE_I2_LOCALIZATION
        localize.Term = "subTitle/" + str;
#else
            uiText.text = str;
#endif
            if (!started)
            {
                fadeColor.FadeIn(uiText, fadeColor.FadeSpeedBase);
            }
            started = true;
        }
    }
}