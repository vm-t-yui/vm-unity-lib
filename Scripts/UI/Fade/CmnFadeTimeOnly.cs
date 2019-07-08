/******************************************************************************/
/*!    \brief  フェード（時間のみ）.
*******************************************************************************/


namespace VMUnityLib
{
    public sealed class CmnFadeTimeOnly : ICmnFade
    {
        /// <summary>
        /// 初期化.
        /// </summary>
        protected override void Start()
        {
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
            }
        }

        /// <summary>
        /// フェードイン開始.
        /// </summary>
        public override void StartFadeIn(EndFadeCallBack callBack, float time)
        {
            StartFadeInInternal(callBack, time);
        }

        /// <summary>
        /// フェードアウト開始.
        /// </summary>
        public override void StartFadeOut(EndFadeCallBack callBack, float time)
        {
            StartFadeOutInternal(callBack, time);
        }
    }
}