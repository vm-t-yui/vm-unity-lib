/******************************************************************************/
/*!    \brief  フェード（時間のみ）.
*******************************************************************************/
using System.Collections;
using UnityEngine;

namespace VMUnityLib
{
    public sealed class CmnFadeTimeOnly : ICmnFade
    {
        Coroutine coroutine;

        /// <summary>
        /// 初期化.
        /// </summary>
        protected override void Start()
        {
            gameObject.SetActive(false);    // 初期化が済んだら非表示に.
        }

        /// <summary>
        /// Calc.
        /// </summary>
        IEnumerator FadeCalcCoroutine()
        {
            while (IsStartedFade)
            {
                CalcAmount();
                yield return null;
            }
        }

        /// <summary>
        /// フェードイン開始.
        /// </summary>
        public override void StartFadeIn(EndFadeCallBack callBack, float time)
        {
            StartFadeInInternal(callBack, time);
            if (coroutine != null) { StopCoroutine(coroutine); }
            coroutine = StartCoroutine(FadeCalcCoroutine());
        }

        /// <summary>
        /// フェードアウト開始.
        /// </summary>
        public override void StartFadeOut(EndFadeCallBack callBack, float time)
        {
            StartFadeOutInternal(callBack, time);
            if (coroutine != null) { StopCoroutine(coroutine); }
            coroutine = StartCoroutine(FadeCalcCoroutine());
        }
    }
}