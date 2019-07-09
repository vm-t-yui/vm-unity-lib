/******************************************************************************/
/*!    \brief  汎用リストUI.
*******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using VMUnityLib;
using UnityEngine.UI;

public class UIList : ListedInHierarchyBehaviourManager<UIListItem>
{
    [SerializeField]
    UIListItem listItemPrefab = default;

    [SerializeField]
    VerticalLayoutGroup Content = default;

    // 追加的に選択するかどうかのフラグ（trueならCtrl押しながら選択するのと同じ効果）.
    public bool AddiveSelect { get; set; }

    /// <summary>
    /// 初期化.
    /// </summary>
    public void Start()
    {
        // 処理なし.
    }

    /// <summary>
    /// リストアイテム作成.
    /// </summary>
    public UIListItem CreateListItem()
    {
        UIListItem item = GameObject.Instantiate(listItemPrefab);
        Add(item);
        return item;
    }

    /// <summary>
    /// アイテム追加.
    /// </summary>
    public override void Add(UIListItem item)
    {
        item.SetOwner(this);
        item.transform.SetParent(Content.transform);
        item.GetComponent<RectTransform>().localScale = Vector3.one;
        base.Add(item);
    }

    /// <summary>
    /// アイテム削除.
    /// </summary>
    public override void Remove(UIListItem item)
    {
        base.Remove(item);
        Destroy(item.gameObject);
    }

    /// <summary>
    /// アイテムをリスト上から一時削除.
    /// </summary>
    public void RemoveInList(UIListItem item)
    {
        base.Remove(item);
    }

    /// <summary>
    /// アイテム削除.
    /// </summary>
    public override void RemoveAt(int idx)
    {
        Remove(GetItem(idx));
    }

    /// <summary>
    /// アイテム削除.
    /// </summary>
    public override void Clear()
    {
        while(GetCount() != 0)
        {
            RemoveAt(0);
        }
    }

    /// <summary>
    /// 選択中のアイテムを取得する.
    /// </summary>
    public UIListItem GetSelectedFirstItem()
    {
        foreach (var item in GetAllItem())
        {
            if (item.IsSelected)
            {
                return item;
            }
        }
        return null;
    }

    /// <summary>
    /// 選択中のアイテムを取得する.
    /// </summary>
    public List<UIListItem> GetSelectedItem()
    {
        List<UIListItem> ret = new List<UIListItem>();
        foreach (var item in GetAllItem())
        {
            if (item.IsSelected)
            {
                ret.Add(item);
            }
        }
        return ret;
    }

    /// <summary>
    /// 全チェック.
    /// </summary>
    public void CheckAll()
    {
        foreach (var item in GetAllItem())
        {
            item.SetChecked(true);
        }
    }
    public void UnCheckAll()
    {
        foreach (var item in GetAllItem())
        {
            item.SetChecked(false);
        }
    }

    /// <summary>
    /// 全選択.
    /// </summary>
    public void SelectAll()
    {
        // 追加的選択がオフなら、全選択の時だけ一時的にオンに.
        bool prevAddivSelectFlag = AddiveSelect;
        AddiveSelect = true;
        foreach (var item in GetAllItem())
        {
            item.SetSelected(true);
        }
        AddiveSelect = prevAddivSelectFlag;
    }
    public void UnSelectAll()
    {
        // 追加的選択がオフなら、全選択の時だけ一時的にオンに.
        bool prevAddivSelectFlag = AddiveSelect;
        AddiveSelect = true;
        foreach (var item in GetAllItem())
        {
            item.SetSelected(false);
        }
        AddiveSelect = prevAddivSelectFlag;
    }

    /// <summary>
    /// 選択された通知イベント.
    /// </summary>
    public void OnSelected(UIListItem in_item)
    {
        if (AddiveSelect == false)
        {
            if (in_item.Owner != this)
            {
                Debug.LogError("invalid call.");
                return;
            }
            else
            {
                foreach (var item in GetAllItem())
                {
                    if (item != in_item)
                    {
                        item.SetSelectedDirect(false);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 編集モード入った通知イベント.
    /// </summary>
    public void OnSetEditting(UIListItem in_item)
    {
        if (in_item.Owner != this)
        {
            Debug.LogError("invalid call.");
            return;
        }
        else
        {
            foreach (var item in GetAllItem())
            {
                if (item != in_item)
                {
                    item.SetEdittingDirect(false);
                }
            }
        }
    }

    /// <summary>
    /// 追加的選択フラグ設定.
    /// </summary>
    public void SetAddiveSelect(bool set)
    {
        AddiveSelect = set;
    }

    /// <summary>
    /// アイテム作成(テスト用).
    /// </summary>
    public void TestCreateItem()
    {
        UIListItem item = CreateListItem();
        item.SetText("list:"+GetCount());
    }
}
