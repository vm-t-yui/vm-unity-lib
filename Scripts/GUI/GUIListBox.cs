/******************************************************************************/
/*!    \brief  指定範囲で、複数の文字列を指定スタイルで描画する（ボタン付き）OnGUIの中でのみ動作.
*******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

public sealed class GUIListBox
{
    public List<KeyValuePair<string, GUIStyle>> DrawList { get; private set; }
    public Rect                         DrawRect { get; private set; }
    public int                          CurrentPage { get; private set; }
    public int                          PageMax     { get; private set; }

    private int onePageItemRowNum = 0;
    private int onePageItemColNum = 1;

    private const float INFO_H = 30.0f;
    private const float INFO_LABEL_S = 5.0f;
    private const float INFO_BUTTON_W = 120.0f;
    private const float LABEL_H = 30.0f;
    private const float LABEL_S = 2.0f;

    /// <summary>
    /// コンストラクタ.
    /// </summary>
    public GUIListBox()
    {
        DrawList = new List<KeyValuePair<string, GUIStyle>>();
        CurrentPage = 0;
        PageMax = 1;
    }

    /// <summary>
    /// 描画.
    /// </summary>
    public void OnGUI()
    {
        if(DrawList.Count != 0 && onePageItemRowNum > 0)
        {
            float x = DrawRect.x;
            float y = DrawRect.y;
            GUI.Box(DrawRect,"");

            Rect rect = new Rect(x,y, INFO_BUTTON_W, INFO_H);
            if(GUI.Button(rect, "←"))
            {
                --CurrentPage;
            }
            rect = new Rect(x + INFO_BUTTON_W, y, INFO_BUTTON_W, INFO_H);
            if (GUI.Button(rect, "→"))
            {
                ++CurrentPage;
            }
            if(CurrentPage < 0)
            {
                CurrentPage = PageMax;
            }
            else if(CurrentPage > PageMax)
            {
                CurrentPage = 0;
            }
            rect = new Rect(x + INFO_BUTTON_W * 2 + 10.0f, y, INFO_BUTTON_W * 3, INFO_H);
            {
                GUI.Label(rect, CurrentPage + " / " + PageMax);
            }
            y += INFO_H + INFO_LABEL_S;

            // --------------------------.
            int curRow = 0;
            int curCol = 0;
            int curIdx = 0;
            float colW = DrawRect.width / onePageItemColNum;

            foreach(KeyValuePair<string, GUIStyle> pair in DrawList)
            {
                if (curIdx >= CurrentPage * (onePageItemColNum * onePageItemRowNum))
                {
                    rect = new Rect(x + (colW * curCol), y + ((LABEL_H + LABEL_S) * curRow), colW, LABEL_H);
                    GUI.Label(rect, pair.Key, pair.Value);
                    ++curRow;
                    if (curRow >= onePageItemRowNum)
                    {
                        ++curCol;
                        curRow = 0;
                    }
                    if (curCol >= onePageItemColNum)
                    {
                        break;
                    }
                }
                ++curIdx;
            }
        }
    }

    /// <summary>
    /// 描画物を設定.
    /// </summary>
    public void SetDrawList(List<KeyValuePair<string, GUIStyle>> dict)
    {
        if (DrawList != dict)
        {
            if (DrawList == null)
            {
                CurrentPage = 0;
            }
            DrawList = dict;
            UpdateOnePageItemNum();
        }
    }

    /// <summary>
    /// 描画範囲を設定.
    /// </summary>
    public void SetDrawRect(Rect rect)
    {
        if (DrawRect != rect)
        {
            DrawRect = rect;
            UpdateOnePageItemNum();
        }
    }

    /// <summary>
    /// １ページに表示する列数を設定する.
    /// </summary>
    public void SetColmnNum(int num)
    {
        if (num > 0 && onePageItemColNum != num)
        {
            onePageItemColNum = num;
            UpdatePageMax();
        }
    }

    /// <summary>
    /// 1ページに描画できるアイテム数を更新.
    /// </summary>
    private void UpdateOnePageItemNum()
    {
        float remH = DrawRect.height - INFO_H - INFO_LABEL_S;
        if (remH < 0)
        {
            onePageItemRowNum = 0;
        }
        else
        {
            onePageItemRowNum = (int)(remH / (LABEL_H + LABEL_S)) + 1;
        }
        UpdatePageMax();
    }

    /// <summary>
    /// ページの最大数を更新.
    /// </summary>
    private void UpdatePageMax()
    {
        if (onePageItemRowNum != 0)
        {
            PageMax = DrawList.Count / onePageItemRowNum;
            if (DrawList.Count % onePageItemRowNum == 0)
            {
                --PageMax;
            }
            PageMax /= onePageItemColNum;
            if (CurrentPage > PageMax)
            {
                CurrentPage = PageMax;
            }
        }
        //Debug.Log("onePageItemRowNum:"+ onePageItemRowNum+ " onePageItemColNum:" + onePageItemColNum+ " PageMax:" + PageMax);
    }
}
