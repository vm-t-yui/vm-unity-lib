/******************************************************************************/
/*!    \brief  リストされたアイテムのマネージャー.
*******************************************************************************/

using System.Collections.Generic;

namespace VMUnityLib
{
    public class ListedItemManager<T>
    {
        List<T> itemList = new List<T>();

        /// <summary>
        /// 個数を返す.
        /// </summary>
        public int GetCount()
        {
            return itemList.Count;
        }

        /// <summary>
        /// アイテムを追加する.
        /// </summary>
        public virtual void Add(T item)
        {
            itemList.Add(item);
        }

        /// <summary>
        /// アイテムを追加する.
        /// </summary>
        public virtual void Add(List<T> itemList)
        {
            foreach (var item in itemList)
            {
                Add(item);
            }
        }

        /// <summary>
        /// アイテムをすべて消去する.
        /// </summary>
        public virtual void Clear()
        {
            itemList.Clear();
        }

        /// <summary>
        /// アイテムを消去する.
        /// </summary>
        public virtual void Remove(T item)
        {
            itemList.Remove(item);
        }

        /// <summary>
        /// アイテムを消去する.
        /// </summary>
        public virtual void Remove(List<T> item)
        {
            while (item.Count != 0)
            {
                Remove(item[0]);
            }
        }

        /// <summary>
        /// アイテムを消去する.
        /// </summary>
        public virtual void RemoveAt(int idx)
        {
            itemList.RemoveAt(idx);
        }

        /// <summary>
        /// アイテムを挿入する.
        /// </summary>
        public virtual void Insert(int idx, T item)
        {
            itemList.Insert(idx, item);
        }

        /// <summary>
        /// アイテムを挿入する.
        /// </summary>
        public void Insert(int idx, List<T> itemList)
        {
            int length = itemList.Count;
            for (int i = length-1; i >= 0; --i)
            {
                var item = itemList[i];
                Insert(idx, item);
            }
        }

        /// <summary>
        /// アイテムを取得する.
        /// </summary>
        public T GetItem(int idx)
        {
            if (idx < 0 || idx >= itemList.Count)
            {
                Logger.Error(idx + ":idx over. count:"+ itemList.Count);
                return default(T);
            }
            else
            {
                return itemList[idx];
            }
        }

        /// <summary>
        /// アイテムを含むかどうか.
        /// </summary>
        public bool Contains(T item)
        {
            return itemList.Contains(item);
        }

        /// <summary>
        /// アイテムのインデックスを取得.
        /// </summary>
        public int IndexOf(T item)
        {
            return itemList.IndexOf(item);
        }

        /// <summary>
        /// ソート.
        /// </summary>
        public virtual void Sort(IComparer<T> comparer)
        {
            itemList.Sort(comparer);
        }

        /// <summary>
        /// 全アイテム取得.
        /// </summary>
        public IEnumerable<T> GetAllItem()
        {
            foreach (T item in itemList)
            {
                yield return item;
            }
        }
    }
}