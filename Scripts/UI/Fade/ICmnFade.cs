/******************************************************************************/
/*!    \brief  フェードインターフェース.
*******************************************************************************/

using UnityEngine;

namespace VMUnityLib
{
    public delegate void EndFadeCallBack();

    public abstract class ICmnFade : MonoBehaviour
    {
        public bool IsStartedFade { get; set; }
        public bool IsFadeIn { get; set; }

        /// <summary>
        /// フェードイン開始.
        /// </summary>
        public abstract void StartFadeIn(EndFadeCallBack callBack, float time);

        /// <summary>
        /// フェードアウト開始.
        /// </summary>
        public abstract void StartFadeOut(EndFadeCallBack callBack, float time);

        protected float Amount { get; set; }    // フェード進行度.

        protected abstract void Start();

        float fadeTime;                       // フェード所要時間.
        float fadeStartTime;                  // フェード開始時間.
        protected EndFadeCallBack endFadeCallBack;                // 終了時のコールバック.

        public void OnDisable()
        {
            //Debug.Log("disable:"+name);
        }

        public void OnActive()
        {
            //Debug.Log("active:"+name);
        }

        /// <summary>
        /// フェードイン開始内部処理.
        /// </summary>
        protected void StartFadeInInternal(EndFadeCallBack callBack, float time)
        {
            gameObject.SetActive(true);
            fadeTime = time;
            IsStartedFade = true;
            IsFadeIn = true;
            fadeStartTime = Time.unscaledTime;
            endFadeCallBack = callBack;
            transform.GetComponent<Collider>().enabled = true;
        }

        /// <summary>
        /// フェードアウト開始内部処理.
        /// </summary>
        protected void StartFadeOutInternal(EndFadeCallBack callBack, float time)
        {
            gameObject.SetActive(true);
            fadeTime = time;
            IsStartedFade = true;
            IsFadeIn = false;
            fadeStartTime = Time.unscaledTime;
            endFadeCallBack = callBack;
            transform.GetComponent<Collider>().enabled = true;
        }

        /// <summary>
        /// 進行度計算.
        /// </summary>
        protected bool CalcAmount()
        {
            var isEnd = false;
            if (fadeTime > 0)
            {
                Amount = (Time.unscaledTime - fadeStartTime) / fadeTime;
            }
            else
            {
                Amount = 1.0f;
            }

            if (Amount >= 1.0f)
            {
                Amount = 1.0f;
                IsStartedFade = false;
                if (IsFadeIn)
                {
                    gameObject.SetActive(false);
                    transform.GetComponent<Collider>().enabled = false;
                }
                isEnd = true;
            }

            if (IsFadeIn)
            {
                Amount = 1.0f - Amount;
            }
            return isEnd;
        }
    }
}