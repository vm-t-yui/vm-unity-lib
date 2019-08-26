/******************************************************************************/
/*!    \brief  標準MonoBehaviour（便利機能をラップしたMonoBehaviour）.
*******************************************************************************/

using UnityEngine;

namespace VMUnityLib
{
    public abstract class CmnMonoBehaviour : MonoBehaviour 
    {
        public const string INIT_SCENCE_CHANGE_NAME   = "InitSceneChange";
        public const string FADE_END_NAME             = "OnFadeInEnd";
        public const string FADE_OUT_END_NAME         = "OnFadeOutEnd";
        public const string SCENCE_DEACTIVE_NAME      = "OnSceneDeactive";

        /// <summary>
        /// 初期化.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// シーン切り替え後の初期化.
        /// </summary>
        protected abstract void InitSceneChange();

        /// <summary>
        /// シーンが無効になったとき.
        /// </summary>
        protected abstract void OnSceneDeactive();

        /// <summary>
        /// フェードイン終了後の処理.
        /// </summary>
        protected abstract void OnFadeInEnd();

        /// <summary>
        /// フェードアウト終了後の処理.
        /// </summary>
        protected abstract void OnFadeOutEnd();

        /// <summary>
        /// 更新.
        /// </summary>
        protected abstract void FixedUpdate();
    }
}