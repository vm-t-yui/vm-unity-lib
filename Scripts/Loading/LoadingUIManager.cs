/******************************************************************************/
/*!    \brief  ローディング画面のUIのマネージャー HACK:ライブラリか？.
*******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
        // ロード画面マップ.
        [SerializeField]
        List<UiTypeSet> loadingUiList = default;

#if USE_TWEEN
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
#endif
            loadingUi = loadingUiDict[type].loadingUi;
        }

        /// <summary>
        /// ロード画面を終了する.
        /// </summary>
        public void HideLoadingUI(LibBridgeInfo.LoadingType type)
        {
#if USE_TWEEN
            loadingUiDict[type].tweenAlpha.Play(PlayDirection.Reverse);
#endif
        }
    }
}