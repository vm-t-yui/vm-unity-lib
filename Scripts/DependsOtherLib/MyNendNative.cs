/******************************************************************************/
/*!    \brief  nendのネイティブのみの表示
*******************************************************************************/
// エディタでは使わないけど保存しておきたい
#pragma warning disable 0414

using UnityEngine;
#if USE_NEND
using NendUnityPlugin.AD.Native;
#endif

namespace VMUnityLib
{
    public class MyNendNative : MonoBehaviour
    {
#if USE_NEND
        [SerializeField]
        NendAdNativeView nativeView = default;

        [SerializeField]
        NendAdNative native = default;

        /// <summary>
        /// 開始時処理
        /// </summary>
        void Start()
        {
            native.LoadAd();
        }

        /// <summary>
        /// 表示非表示の切り替え
        /// </summary>
        /// <param name="isShow"></param>
        public void Show(bool isShow)
        {
//#if !UNITY_EDITOR
            nativeView.gameObject.SetActive(isShow);
//#endif
        }
#endif
    }
}