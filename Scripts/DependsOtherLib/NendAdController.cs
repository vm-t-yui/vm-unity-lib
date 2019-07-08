/******************************************************************************/
/*!    \brief  nendのアドを表示したり消したりする.
*******************************************************************************/

using UnityEngine;
using VMUnityLib;
#if USE_NEND
using NendUnityPlugin.AD.Native.Utils;
using NendUnityPlugin.AD;
#endif
namespace VMUnityLib
{
    public sealed class NendAdController : SingletonMonoBehaviour<NendAdController>
    {
#if USE_NEND
        [SerializeField]
        private NendAdBanner topBanner;

        [SerializeField]
        private NendAdBanner bottomBanner;

        [SerializeField]
        private NendAdInterstitial inter;

        public bool IsShowTopBanner { get; private set; }
        public bool IsShowBottomBanner { get; private set; }

        /// <summary>
        /// スタート.
        /// </summary>
        public void Start()
        {
            NendAdLogger.LogLevel = NendAdLogger.NendAdLogLevel.Warn;
#if UNITY_IPHONE
        NendAdInterstitial.Instance.Load("308c2499c75c4a192f03c02b2fcebd16dcb45cc9", "213208"); // Debug.
        //NendAdInterstitial.Instance.Load("b236649e38d1bf465e41ee7ea28b98577f0ace49", "618540");
#elif UNITY_ANDROID
        NendAdInterstitial.Instance.Load("8c278673ac6f676dae60a1f56d16dad122e23516", "213206"); // Debug.
        //NendAdInterstitial.Instance.Load("349ad8e243426373edb3e9ccf42e4ec113c732b8", "607621");
#endif
            IsShowTopBanner = false;
            IsShowBottomBanner = false;
        }

        /// <summary>
        /// トップバナーの表示管理.
        /// </summary>
        public void ShowTopBanner(bool show)
        {
            if (show)
                topBanner.Show();
            else
                topBanner.Hide();
            IsShowTopBanner = show;
        }

        /// <summary>
        /// ボトムバナーの表示管理.
        /// </summary>
        public void ShowBottomBanner(bool show)
        {
            if (show)
                bottomBanner.Show();
            else
                bottomBanner.Hide();
            IsShowBottomBanner = show;
        }

        /// <summary>
        /// インターステシャル広告の表示管理.
        /// </summary>
        public void ShowInterstitial()
        {
            inter.Show();
        }
#endif
    }
}
