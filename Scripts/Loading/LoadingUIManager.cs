/******************************************************************************/
/*!    \brief  ローディング画面のUIのマネージャー HACK:ライブラリか？.
*******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace VMUnityLib
{
    public sealed class LoadingUIManager : SingletonMonoBehaviour<LoadingUIManager>
    {
        [System.Serializable]
        class UiTypeSet
        {
            public LibBridgeInfo.LoadingType type = default;
            public LoadingUiBase loadingUi = default;
#if USE_TWEEN
            public uTweenAlpha tweenAlpha = default;
#endif
        }

#if USE_TWEEN
        // ロード画面マップ.
        [SerializeField]
        List<UiTypeSet> loadingUiList = default;

        Dictionary<LibBridgeInfo.LoadingType, UiTypeSet> loadingUiDict;
#endif

        void Start()
        {
#if USE_TWEEN
            loadingUiDict = new Dictionary<LibBridgeInfo.LoadingType, UiTypeSet>();
            loadingUiDict = loadingUiList.ToDictionary(ui => ui.type);
#endif
        }

        /// <summary>
        /// ロード画面を表示する.
        /// </summary>
        public void ShowLoadingUI(LibBridgeInfo.LoadingType type,out LoadingUiBase loadingUi)
        {
#if USE_TWEEN
            loadingUiDict[type].loadingUi.gameObject.SetActive(true);
            loadingUiDict[type].tweenAlpha.Play(PlayDirection.Forward);
            loadingUi = loadingUiDict[type].loadingUi;
#else
            loadingUi = null;
#endif
        }

        /// <summary>
        /// ロード画面を終了する.
        /// </summary>
        public void HideLoadingUI(LibBridgeInfo.LoadingType type)
        {
#if USE_TWEEN
            // フェードアウト
            loadingUiDict[type].tweenAlpha.Play(PlayDirection.Reverse);
            // フェードアウトが終わったら、オブジェクトをオフにする
            UnityAction onEndFadeout = null;
            loadingUiDict[type].tweenAlpha.onFinished.AddListener(onEndFadeout = () =>
            {
                loadingUiDict[type].tweenAlpha.gameObject.SetActive(false);
                loadingUiDict[type].tweenAlpha.onFinished.RemoveListener(onEndFadeout);
            });
#endif
        }
    }
}