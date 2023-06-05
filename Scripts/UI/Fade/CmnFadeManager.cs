/******************************************************************************/
/*!    \brief  フェードのマネージャー.
*******************************************************************************/

using UnityEngine;

namespace VMUnityLib
{
    public sealed class CmnFadeManager : SingletonMonoBehaviour<CmnFadeManager>
    {
        public enum FadeType
        {
            FADE_TIMEONLY,  // 時間のみのフェード.
            FADE_NORMAL,    // 共通のイメージフェード.
            FADE_COLOR      // カラーのみのフェード.
        }

        [SerializeField]
        CmnFadeNormal normalFade = default;
        [SerializeField]
        CmnFadeTimeOnly timeFade = default;
        [SerializeField]
        CmnFadeNormal colorFade = default;

        /// <summary>
        /// フェードイン開始.
        /// </summary>
        public void StartFadeIn(EndFadeCallBack callBack, float time, FadeType type)
        {
            StartFadeIn(callBack, time, type, Color.white);
        }
        public void StartFadeIn(EndFadeCallBack callBack, float time, FadeType type, Color color)
        {
            switch (type)
            {
                case FadeType.FADE_TIMEONLY:
                    timeFade.StartFadeIn(callBack, time);
                    break;
                case FadeType.FADE_NORMAL:
                    normalFade.Color = color;
                    normalFade.StartFadeIn(callBack, time);
                    break;
                case FadeType.FADE_COLOR:
                    colorFade.Color = color;
                    colorFade.StartFadeIn(callBack, time);
                    break;
            }
        }

        /// <summary>
        /// フェードアウト開始.
        /// </summary>
        public void StartFadeOut(EndFadeCallBack callBack, float time, FadeType type)
        {
            StartFadeOut(callBack, time, type, Color.white);
        }
        public void StartFadeOut(EndFadeCallBack callBack, float time, FadeType type, Color color)
        {
            // EventManager.Inst.InvokeEvent(SubjectType.StartFadeOut);
            switch (type)
            {
                case FadeType.FADE_TIMEONLY:
                    timeFade.StartFadeOut(callBack, time);
                    break;
                case FadeType.FADE_NORMAL:
                    normalFade.Color = color;
                    normalFade.StartFadeOut(callBack, time);
                    break;
                case FadeType.FADE_COLOR:
                    colorFade.Color = color;
                    colorFade.StartFadeOut(callBack, time);
                    break;
            }
        }
    }
}