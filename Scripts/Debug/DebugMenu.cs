/******************************************************************************/
/*!    \brief  デバッグを管理する.
*******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace VMUnityLib
{
    public sealed class DebugMenu : MonoBehaviour
    {
        public AllocationStats alocationStat;

        bool isDebugWindowOpen = false;
        List<DebugMenuPage> pages = new List<DebugMenuPage>();
        int nowPageIndex = 0;

        /// <summary>
        /// Awake this instance.
        /// </summary>
        void Awake()
        {
            pages = LibBridgeInfo.GetDebugMenuPages();
            nowPageIndex = 0;
        }

        /// <summary>
        /// Use this for initialization.
        /// </summary>
        void Start()
        {
        }

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        void FixedUpdate()
        {
        }

        /// <summary>
        /// Raises the GU event.
        /// </summary>
        public void OnGUI()
        {
            GUIHelper.SetAllFontSizeToGameScreen();

            // 常駐のデバッグボタン.
            if (isDebugWindowOpen == false)
            {
                float w = 120; float h = 100;
                Rect rect = GUIHelper.GetScaledRect(0, 0, w, h);
                if (GUI.Button(rect, "debug"))
                {
                    isDebugWindowOpen = true;
                }
            }
            else
            {
                GUI.Box(GUIHelper.GetScaledRect(0, 0, LibBridgeInfo.FIXED_SCREEN_WI, LibBridgeInfo.FIXED_SCREEN_HI), "デバッグメニュー(" + pages[nowPageIndex].GetPageName() + ")");

                // 閉じる.
                float x = 5; float y = 5;
                float w = 80; float h = 80;
                float spaceH = 10;
                float spaceW = 10;
                if (GUI.Button(GUIHelper.GetScaledRect(x, y, w, h), "×"))
                {
                    isDebugWindowOpen = false;
                }

                // ステータス表記.
                x += w + spaceW;
                w = 160;
                if (GUI.Button(GUIHelper.GetScaledRect(x, y, w, h), "Stats"))
                {
                    if (alocationStat)
                    {
                        alocationStat.gameObject.SetActive(!alocationStat.gameObject.activeSelf);
                    }
                }

                // ページ送り.
                x += w + spaceW;
                w = 100;
                bool pageChanged = false;
                int prevIndex = nowPageIndex;
                if (GUI.Button(GUIHelper.GetScaledRect(x, y, w, h), "[←]"))
                {
                    --nowPageIndex;
                    pageChanged = true;
                }
                x += w + spaceW;
                if (GUI.Button(GUIHelper.GetScaledRect(x, y, w, h), "[→]"))
                {
                    ++nowPageIndex;
                    pageChanged = true;
                }
                y += h + spaceH;

                if (nowPageIndex < 0)
                {
                    nowPageIndex = pages.Count - 1;
                }
                else if (nowPageIndex >= pages.Count)
                {
                    nowPageIndex = 0;
                }
                if (pageChanged)
                {
                    pages[prevIndex].OnDeactive();
                    pages[nowPageIndex].OnActive();
                }

                y += spaceH;

                // ページ描画.
                if (pages[nowPageIndex].OnGUI(y))
                {
                    isDebugWindowOpen = false;
                }
            }
        }
    }
}