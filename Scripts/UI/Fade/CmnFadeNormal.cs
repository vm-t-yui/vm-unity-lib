/******************************************************************************/
/*!    \brief  通常のフェード.
*******************************************************************************/

using UnityEngine;
using UnityEngine.UI;

namespace VMUnityLib
{
    public sealed class CmnFadeNormal : ICmnFade
    {
        public  Color Color { get; set; }             // フェードするオブジェクトのカラー.
        private Image overlay;                        // フェード用の上に重ねるオブジェクト.

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
        /// Update.
        /// </summary>
        protected override void FixedUpdate()
        {
            if (IsStartedFade)
            {
                CalcAmount();
                overlay.color = new Color(Color.r, Color.g, Color.b, Amount);
            }
        }

        /// <summary>
        /// フェードイン開始.
        /// </summary>
        public override void StartFadeIn(EndFadeCallBack callBack, float time)
        {
            StartFadeInInternal(callBack, time);
            overlay.color = new Color(Color.r, Color.g, Color.b, 1.0f);
        }

        /// <summary>
        /// フェードアウト開始.
        /// </summary>
        public override void StartFadeOut(EndFadeCallBack callBack, float time)
        {
            StartFadeOutInternal(callBack, time);
            overlay.color = new Color(Color.r, Color.g, Color.b, 0.0f);
        }
    }
}