/******************************************************************************/
/*!    \brief  デバッグメニューのページ用抽象クラス.
*******************************************************************************/


namespace VMUnityLib
{
    public abstract class DebugMenuPage
    {
        public abstract string GetPageName(); // ページ名.

        /// <summary>
        /// 描画本体.
        /// </summary>
        /// <param name="startY">描画開始Y座標.</param>
        /// <returns>メニューを閉じるかどうか.</returns>
        public abstract bool OnGUI(float startY);

        /// <summary>
        /// 有効・無効時イベント.
        /// </summary>
        public abstract void OnActive();
        public abstract void OnDeactive();
    }
}