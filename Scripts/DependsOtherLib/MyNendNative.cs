/******************************************************************************/
/*!    \brief  nendのネイティブのみの表示
*******************************************************************************/
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
        NendAdNativeView nativeView;
        [SerializeField]
        NendAdNative native;

        /// <summary>
        /// 開始時処理
        /// </summary>
        private void Start()
        {
            native.LoadAd();
        }

        /// <summary>
        /// 表示非表示の切り替え
        /// </summary>
        /// <param name="isShow"></param>
        public void Show(bool isShow)
        {
#if !UNITY_EDITOR
        nativeView.gameObject.SetActive(isShow);
#endif
        }
#endif
    }
}