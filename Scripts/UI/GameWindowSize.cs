/******************************************************************************/
/*!    \brief  ゲームの表示領域を計算して格納する.
*******************************************************************************/

#if UNITY_EDITOR
#endif

using UnityEngine;

namespace VMUnityLib
{
    public sealed class GameWindowSize
    {
        private static GameWindowSize inst = null; 
        private static GameWindowSize Inst 
        { 
            get
            {
    #if UNITY_EDITOR
                // エディタなら自動確保.
                if(inst == null)
                {
                    inst = new GameWindowSize(LibBridgeInfo.FIXED_SCREEN_WI, LibBridgeInfo.FIXED_SCREEN_HI);
                }
    #endif
                return inst;
            }
            set { inst = value; }
        }
    
        private float scaleW;           // 画面サイズのスケール比(幅）.
        private float scaleH;           // 画面サイズのスケール比（高さ）.
        private float gameScreenScale;  // ゲーム表示領域のFixedSizeに対するスケール値.
        private float gameScreenW;      // ゲーム表示領域のW.
        private float gameScreenH;      // ゲーム表示領域のH.
        private float spaceW;           // 画面サイズに対してのw余白.
        private float spaceH;           // 画面サイズに対してのh余白.

	    public GameWindowSize (
		    int fixWidth, 
		    int fixHeight
		    )
	    {
		    Calc (fixWidth, fixHeight);
	    }
    
        /// <summary>
        /// ウインドウサイズを取得.
        /// </summary>
	    private void Calc (int w, int h)
	    {
            float fixSizeScale = 1.0f;
    #if UNITY_WEBPLAYER && (!UNITY_EDITOR)
            scaleW = 1.0f;
            scaleH = 1.0f;
    #else
            scaleW = (float)Screen.width / w;
            scaleH = (float)Screen.height / h;
            if(w > h)
            {
                fixSizeScale = scaleH;
            }
            else
            {
                fixSizeScale = scaleW;
            }
    #endif
            spaceW = ((float)Screen.width - (w * fixSizeScale)) * 0.5f;
            spaceH = ((float)Screen.height - (h * fixSizeScale)) * 0.5f;
            gameScreenW = Screen.width - spaceW * 2;
            gameScreenH = Screen.height - spaceH * 2;
            if (w > h) 
            {
                gameScreenScale = gameScreenW / w;
            } 
            else 
            {
                gameScreenScale = gameScreenH / h;
            }
	    }
    

        //---------------------------------------------------------------------------//
        // static 関数.
        //---------------------------------------------------------------------------//

        static public float GameScreenSpaceW       { get { return Inst.spaceW * Inst.scaleW; } }
        static public float GameScreenSpaceH       { get { return Inst.spaceH * Inst.scaleH; } }
        static public float GameScreenTrueSpaceW   { get { return Inst.spaceW; } }
        static public float GameScreenTrueSpaceH   { get { return Inst.spaceH; } }
        static public float GameScreenTrueW        { get { return Inst.gameScreenW; } }
        static public float GameScreenTrueH        { get { return Inst.gameScreenH; } }
        static public float GameScreenScale        { get { return Inst.gameScreenScale; } }

        /// <summary>
        /// 初期化・インスタンス生成.
        /// </summary>
        static public void Init(
            int fixWidth, 
            int fixHeight
            )
        {
            inst = new GameWindowSize (fixWidth, fixHeight);
        }
    
        /// <summary>
        /// ゲームサイズの再計算.
        /// </summary>
        public static void ReCalc(int w, int h)
        {
            Inst.Calc (w, h);
        }

        /// <summary>
        /// ゲームスクリーン上での矩形を取得する.
        /// </summary>
	    public static Rect GetRect (float x, float y, float width, float height)
	    { 
		    Rect rect 
			    = new Rect 
				    (
                        (Inst.spaceW + x) * Inst.gameScreenScale,
                        (Inst.spaceH + y) * Inst.gameScreenScale, 
                        width * Inst.gameScreenScale, 
                        height * Inst.gameScreenScale
					    );
		
		    return rect;
		
	    }

        /// <summary>
        /// ゲームスクリーン上での矩形を取得する.
        /// </summary>
	    public static Rect GetRect (Rect rect)
	    {
		    return GetRect (rect.x, rect.y, rect.width, rect.height);
        }

        /// <summary>
        /// ゲームスクリーン上での矩形（スペースなし）を取得する.
        /// </summary>
        public static Rect GetRectWithoutSpace(float x, float y, float width, float height)
        {
            Rect rect
                = new Rect
                    (
                        x * Inst.gameScreenScale,
                        y * Inst.gameScreenScale,
                        width * Inst.gameScreenScale,
                        height * Inst.gameScreenScale
                        );

            return rect;

        }

        /// <summary>
        /// ゲームスクリーン上での矩形（スペースなし）を取得する.
        /// </summary>
        public static Rect GetRectWithoutSpace(Rect rect)
        {
            return GetRectWithoutSpace(rect.x, rect.y, rect.width, rect.height);
        }
    }
}