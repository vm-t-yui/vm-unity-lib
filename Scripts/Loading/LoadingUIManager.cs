/******************************************************************************/
/*!    \brief  ローディング画面のUIのマネージャー HACK:ライブラリか？.
*******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VMUnityLib;

namespace VMUnityLib
{
    public sealed class LoadingUIManager : SingletonMonoBehaviour<LoadingUIManager>
    {
        [System.Serializable]
        struct UiTypeSet
        {
            public LibBridgeInfo.LoadingType type;
#if USE_TWEEN
            public uTweenAlpha tweenAlpha;
#endif
        }
        // ロード画面マップ.
        [SerializeField]
        List<UiTypeSet> loadingUiList;

#if USE_TWEEN
        Dictionary<LibBridgeInfo.LoadingType, uTweenAlpha> loadingUiDict;
#endif

        void Start()
        {
#if USE_TWEEN
            loadingUiDict = new Dictionary<LibBridgeInfo.LoadingType, uTweenAlpha>();
            foreach (UiTypeSet item in loadingUiList)
            {
                loadingUiDict.Add(item.type, item.tweenAlpha);
            }
#endif
        }

        /// <summary>
        /// ロード画面を表示する.
        /// </summary>
        public void ShowLoadingUI(LibBridgeInfo.LoadingType type)
        {
#if USE_TWEEN
            loadingUiDict[type].gameObject.SetActive(true);
            loadingUiDict[type].Play(PlayDirection.Forward);
#endif
        }

        /// <summary>
        /// ロード画面を終了する.
        /// </summary>
        public void HideLoadingUI(LibBridgeInfo.LoadingType type)
        {
#if USE_TWEEN
            loadingUiDict[type].Play(PlayDirection.Reverse);
#endif
        }
    }
}