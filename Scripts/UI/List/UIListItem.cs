/******************************************************************************/
/*!    \brief  汎用リストアイテム.
*******************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class UIListItem : MonoBehaviour 
{
    [SerializeField] protected Text label;             
    [SerializeField] protected Toggle editToggle;      
    [SerializeField] protected Toggle checkToggle; 
    [SerializeField] private bool enableEdit;           // Editの表示が行われるかどうか.
    [SerializeField] private bool selectCheckSync;      // 選択とチェックマークの同期がされるかどうか.

    private Toggle myToggle;

    public UIList Owner { get; private set; }
    public bool IsChecked { get; private set; }
    public bool IsSelected { get; private set; }
    public bool ToggleSelectEventLock { get; set; }
    public bool ToggleEditEventLock { get; set; }

    public string Text { get; private set; }

    /// <summary>
    /// 生成時.
    /// </summary>
    private void Awake()
    {
        myToggle = GetComponent<Toggle>();
    }

    /// <summary>
    /// 初期化.
    /// </summary>
    public void Start()
    {
        if (enableEdit == false)
        {
            editToggle.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// オーナーをセット.
    /// </summary>
    public void SetOwner(UIList list)
    {
        Owner = list;
    }

    /// <summary>
    /// テキストセット.
    /// </summary>
    public void SetText(string t)
    {
        Text = t;
        UpdateLabel();
    }

    /// <summary>
    /// テキストアップデート.
    /// </summary>
    private void UpdateLabel()
    {
        label.text = Text;
        //if (IsSelected)
        //{
        //    if (IsChecked)
        //    {
        //        label.text = Text + " *x";
        //    }
        //    else
        //    {
        //        label.text = Text + " *";
        //    }
        //}
        //else
        //{
        //    if (IsChecked)
        //    {
        //        label.text = Text + " x";
        //    }
        //    else
        //    {
        //        label.text = Text;
        //    }
        //}
    }

    /// <summary>
    /// セレクト状態を設定（スクリプト）.
    /// </summary>
    public void SetSelected(bool set)
    {
        myToggle.isOn = set;
    }
    public void SetSelectedDirect(bool set)
    {
        IsSelected = set;
        //Debug.Log("setD"+ Text + " :"+ IsSelected);
        ToggleSelectEventLock = true;
        myToggle.isOn = set;
        ToggleSelectEventLock = false;
        UpdateLabel();
    }

    /// <summary>
    /// セレクト状態を設定(インスペクタ).
    /// </summary>
    public void SetSelected(Toggle toggle)
    {
        if (ToggleSelectEventLock == false)
        {
            editToggle.isOn = false;
            IsSelected = toggle.isOn;
            //Debug.Log("setI" + Text + " :" + IsSelected);
            Owner.OnSelected(this);
            UpdateLabel();
            if(selectCheckSync)
            {
                SetChecked(IsSelected);
            }
        }
    }

    /// <summary>
    /// チェック状態を設定（スクリプト）.
    /// </summary>
    public void SetChecked(bool set)
    {
        checkToggle.isOn = set;
    }

    /// <summary>
    /// チェック状態を設定(インスペクタ).
    /// </summary>
    public void SetChecked(Toggle toggle)
    {
        IsChecked = toggle.isOn;
        editToggle.isOn = false;
        UpdateLabel();
        //Debug.Log("IsChecked:" + IsChecked);
    }

    /// <summary>
    /// リストエディット状態を設定（スクリプト）.
    /// </summary>
    public void SetEditting(bool set)
    {
        editToggle.isOn = set;
    }
    public void SetEdittingDirect(bool set)
    {
        ToggleEditEventLock = true;
        editToggle.isOn = set;
        ToggleEditEventLock = false;
    }

    /// <summary>
    /// リストエディット状態を設定(インスペクタ).
    /// </summary>
    public void SetEditting(Toggle toggle)
    {
        if (ToggleEditEventLock == false)
        {
            Owner.OnSetEditting(this);
        }
    }

    /// <summary>
    /// リムーブを要求.
    /// </summary>
    public void RequestRemoveSelf()
    {
        //Debug.Log("RequestRemoveSelf");
        Owner.Remove(this);
    }

    /// <summary>
    /// 上方向への要素移動をリクエスト.
    /// </summary>
    public void RequestMoveUp()
    {
        //Debug.Log("RequestMoveUp");
        int idx = Owner.IndexOf(this);
        if (idx > 0)
        {
            Owner.RemoveInList(this);
            Owner.Insert(idx - 1, this);
        }
    }

    /// <summary>
    /// 下方向への要素移動をリクエスト.
    /// </summary>
    public void RequestMoveDown()
    {
        //Debug.Log("RequestMoveDown");
        int idx = Owner.IndexOf(this);
        if (idx < Owner.GetCount() - 1)
        {
            Owner.RemoveInList(this);
            Owner.Insert(idx + 1, this);
        }
    }
}
