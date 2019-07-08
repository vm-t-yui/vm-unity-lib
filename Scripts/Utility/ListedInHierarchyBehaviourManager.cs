/******************************************************************************/
/*!    \brief  リストされたアイテムのマネージャー（自身の下で管理するtransform版）.
*******************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace VMUnityLib
{
    public class ListedInHierarchyBehaviourManager<T> : ListedBehaviourManager<T> where T : MonoBehaviour
    {
        /// <summary>
        /// アイテムを追加する.
        /// </summary>
        public override void Add(T item)
        {
            base.Add(item);
            SortSiblingIndex();
        }

        /// <summary>
        /// アイテムを消去する.
        /// </summary>
        public override void Remove(T item)
        {
            base.Remove(item);
            SortSiblingIndex();
        }

        /// <summary>
        /// アイテムを消去する.
        /// </summary>
        public override void RemoveAt(int idx)
        {
            base.RemoveAt(idx);
            SortSiblingIndex();
        }

        /// <summary>
        /// アイテムを挿入する.
        /// </summary>
        public override void Insert(int idx, T item)
        {
            base.Insert(idx,item);
            SortSiblingIndex();
        }

        /// <summary>
        /// ソート.
        /// </summary>
        public override void Sort(IComparer<T> comparer)
        {
            base.Sort(comparer);
            SortSiblingIndex();
        }

        /// <summary>
        /// ヒエラルキー上の順序を変える.
        /// </summary>
        private void SortSiblingIndex()
        {
            int i = 0;
            foreach (MonoBehaviour item in GetAllItem())
            {
                item.transform.SetSiblingIndex(i);
                ++i;
            }
        }
    }
}