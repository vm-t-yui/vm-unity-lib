/******************************************************************************/
/*!    \brief  通常のフェード.
*******************************************************************************/
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace VMUnityLib
{
    /// <summary>
    /// Unity3時代のモノなのでTweenもコルーチンも何もかも使わなかった時代の化石
    /// </summary>
    public sealed class CmnFadeNormal : ICmnFade
    {
        public Color Color { get; set; }             // フェードするオブジェクトのカラー.
        Image overlay;                        // フェード用の上に重ねるオブジェクト.
        Coroutine coroutine;

        /// <summary>
        /// 初期化.
        /// </summary>
        protected override void Start()
        {
            Color = Color.black;
            overlay = GetComponent<Image>();
            overlay.color = new Color(Color.r, Color.g, Color.b, 0);
            gameObject.SetActive(false);    // 初期化が済んだら非表示に.
        }

        /// <summary>
        /// Calc.
        /// </summary>
        IEnumerator FadeCalcCoroutine()
        {
            while(IsStartedFade)
            {
                CalcAmount();
                overlay.color = new Color(Color.r, Color.g, Color.b, Amount);
                yield return null;
            }
        }

        /// <summary>
        /// フェードイン開始.
        /// </summary>
        public override void StartFadeIn(EndFadeCallBack callBack, float time)
        {
            if(time == 0)
            {
                overlay.color = new Color(Color.r, Color.g, Color.b, 0.0f);
                return;
            }
            StartFadeInInternal(callBack, time);
            if(coroutine != null) { StopCoroutine(coroutine); }
            coroutine = StartCoroutine(FadeCalcCoroutine());
            overlay.color = new Color(Color.r, Color.g, Color.b, 1.0f);
        }

        /// <summary>
        /// フェードアウト開始.
        /// </summary>
        public override void StartFadeOut(EndFadeCallBack callBack, float time)
        {
            if (time == 0)
            {
                overlay.color = new Color(Color.r, Color.g, Color.b, 1.0f);
                return;
            }
            StartFadeOutInternal(callBack, time);
            if(coroutine != null) { StopCoroutine(coroutine); }
            coroutine = StartCoroutine(FadeCalcCoroutine());
            overlay.color = new Color(Color.r, Color.g, Color.b, 0.0f);
        }
    }
}