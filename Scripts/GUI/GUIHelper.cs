/******************************************************************************/
/*!    \brief  GUILayoutのヘルパー.
*******************************************************************************/

using UnityEngine;
namespace VMUnityLib
{
    public sealed class GUIHelper
    {
        /// <summary>
        /// 全てのGUIStyleのフォントサイズをゲームスクリーンに合わせて設定する.
        /// </summary>
        public static void SetAllFontSizeToGameScreen()
        {
            foreach (GUIStyle style in GUI.skin)
            {
                SetFontSizeToGameScreen(style);
            }
        }

        /// <summary>
        /// 指定のGUIStyleのフォントサイズをゲームスクリーンに合わせて設定する.
        /// </summary>
        public static void SetFontSizeToGameScreen(GUIStyle style)
        {
            style.fontSize = (int)(20 * GameWindowSize.GameScreenScale);
        }

        /// <summary>
        /// ゲームスクリーンサイズに補正された値を返す.
        /// </summary>
        public static float GetScaledSize(float f)
        {
            return GameWindowSize.GameScreenScale * f;
        }

        /// <summary>
        /// ゲームスクリーンサイズに補正された矩形を返す.
        /// </summary>
        public static Rect GetScaledRect(float x, float y, float w, float h)
        {
            return GameWindowSize.GetRect(x, y, w, h);
        }
        public static Rect GetScaledRect(Rect rect)
        {
            return GameWindowSize.GetRect(rect);
        }
        public static Rect GetScaledRectWithoutSpace(float x, float y, float w, float h)
        {
            return GameWindowSize.GetRectWithoutSpace(x, y, w, h);
        }
        public static Rect GetScaledRectWithoutSpace(Rect rect)
        {
            return GameWindowSize.GetRectWithoutSpace(rect);
        }
    }
}