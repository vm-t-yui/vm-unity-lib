/******************************************************************************/
/*!    \brief  ID付けされたデータのマネージャー.
*******************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace VMUnityLib
{
    public class IdentifiedDataManager<T> : IdentifiedObjectManager<T> where T : BaseData
    {
        public string DataPath { get; private set; }

        /// <summary>
        /// コンストラクタ.
        /// </summary>
        /// <param name="dataPath">データを管理するパス</param>
        public IdentifiedDataManager(string dataPath)
        {
            DataPath = dataPath;
        }

        /// <summary>
        /// データをリロードする.
        /// </summary>
        public virtual void ReloadData()
        {
            RemoveAllData();
            LoadData();
        }

        /// <summary>
        /// データをロードする.
        /// </summary>
        public virtual void LoadData()
        {
            T[] datas = Resources.LoadAll<T>(DataPath);
            foreach (T data in datas)
            {
                AddData(data, data.GetId());
            }
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// データを追加する.
        /// </summary>
        public void AddData(T data, string id)
        {
            AddObject(data, id);
        }

        /// <summary>
        /// データをすべて消去する.
        /// </summary>
        public void RemoveAllData()
        {
            RemoveAllObject();
        }

        /// <summary>
        /// IDを元にデータを取得する.
        /// </summary>
        public bool GetData(string id, out T data)
        {
            return GetObject(id, out data);
        }

        /// <summary>
        /// データが存在するかどうか.
        /// </summary>
        public bool IsExistData(string id)
        {
            return IsExistObject(id);
        }

        /// <summary>
        /// 全データ取得.
        /// </summary>
        public IEnumerable<T> GetAllData()
        {
            return GetAllObject();
        }
    }
}