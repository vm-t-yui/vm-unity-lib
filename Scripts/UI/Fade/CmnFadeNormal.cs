/******************************************************************************/
/*!    \brief  通常のフェード.
*******************************************************************************/
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace VMUnityLib
{
    /// <summary>
    /// Unity3時代のTweenもコルーチンも何もかも使わなかった時代の挙動と整合性とるので少し変
    /// </summary>
    public sealed class CmnFadeNormal : ICmnFade
    {
        public Color FadeInColor  { get; set; }         // フェードするオブジェクトのカラー
        public Color FadeOutColor { get; set; }         // フェードするオブジェクトのカラー
        public Color Color { set { FadeInColor = FadeOutColor = value; } }
        public float ColorChangeTime { get; set; }
        Image overlay;                                  // フェード用の上に重ねるオブジェクト
        Coroutine coroutine;

        /// <summary>
        /// 初期化
        /// </summary>
        protected override void Start()
        {
            Color = Color.black;
            overlay = GetComponent<Image>();
            overlay.color = new Color(FadeInColor.r, FadeInColor.g, FadeInColor.b, 0);
            gameObject.SetActive(false);    // 初期化が済んだら非表示に.
        }

        /// <summary>
        /// Calc
        /// </summary>
        IEnumerator FadeCalcCoroutine(Color color)
        {
            while(IsStartedFade)
            {
                var isEnd = CalcAmount();
                overlay.color = new Color(color.r, color.g, color.b, Amount);
                if(isEnd)
                {
                    endFadeCallBack?.Invoke();
                }
                yield return null;
            }
        }

        /// <summary>
        /// カラーチェンジ付きフェードインコルーチン
        /// </summary>
        IEnumerator ColorChangeFadeInCoroutine(EndFadeCallBack callBack, float fadeInTime, Color from, Color to)
        {
            float changeStartTime = Time.unscaledTime;
            float t = 0;

            while(t < 1.0f)
            {
                if (ColorChangeTime > 0)
                {
                    t = (Time.unscaledTime - changeStartTime) / ColorChangeTime;
                }
                else
                {
                    t = 1.0f;
                }
                if (t >= 1.0f)
                {
                    t = 1.0f;
                }
                overlay.color = Color.Lerp(from, to, t);
                yield return null;
            }
            StartFadeInInternal(callBack, fadeInTime);
            yield return FadeCalcCoroutine(FadeInColor);
        }

        /// <summary>
        /// フェードイン開始
        /// </summary>
        public override void StartFadeIn(EndFadeCallBack callBack, float time)
        {
            if(time == 0)
            {
                overlay.color = new Color(FadeInColor.r, FadeInColor.g, FadeInColor.b, 0.0f);
                return;
            }
            
            if (coroutine != null) { StopCoroutine(coroutine); }

            // フェードインとアウトが違えばカラーチェンジ
            if (FadeOutColor != FadeInColor)
            {
                overlay.color = new Color(FadeOutColor.r, FadeOutColor.g, FadeOutColor.b, 0.0f);
                coroutine = StartCoroutine(ColorChangeFadeInCoroutine(callBack, time, FadeOutColor, FadeInColor));
            }
            else
            {
                StartFadeInInternal(callBack, time);
                coroutine = StartCoroutine(FadeCalcCoroutine(FadeInColor));
                overlay.color = new Color(FadeInColor.r, FadeInColor.g, FadeInColor.b, 1.0f);
            }
        }

        /// <summary>
        /// フェードアウト開始
        /// </summary>
        public override void StartFadeOut(EndFadeCallBack callBack, float time)
        {
            if (time == 0)
            {
                overlay.color = new Color(FadeOutColor.r, FadeOutColor.g, FadeOutColor.b, 1.0f);
                return;
            }
            StartFadeOutInternal(callBack, time);
            if(coroutine != null) { StopCoroutine(coroutine); }
            coroutine = StartCoroutine(FadeCalcCoroutine(FadeOutColor));
            overlay.color = new Color(FadeOutColor.r, FadeOutColor.g, FadeOutColor.b, 0.0f);
        }
    }
}