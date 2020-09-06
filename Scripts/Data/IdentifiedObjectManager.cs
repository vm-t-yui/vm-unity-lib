/******************************************************************************/
/*!    \brief  ID付けされたオブジェクトのマネージャー.
*******************************************************************************/

using System.Collections.Generic;

namespace VMUnityLib
{
    public class IdentifiedObjectManager<T>
    {
        Dictionary<string, T> objectDictionary = new Dictionary<string, T>();

        /// <summary>
        /// オブジェクトを追加する.
        /// </summary>
        public void AddObject(T obj, string id)
        {
            objectDictionary.Add(id, obj);
        }

        /// <summary>
        /// オブジェクトをすべて消去する.
        /// </summary>
        public void RemoveAllObject()
        {
            objectDictionary = new Dictionary<string, T>();
        }

        /// <summary>
        /// IDを元にオブジェクトを取得する.
        /// </summary>
        public bool GetObject(string id, out T obj)
        {
            if (objectDictionary.TryGetValue(id, out obj))
            {
                return true;
            }
            else
            {
#if DEBUG && LOG_TRACE
                VMLogger.Error(id + " does not exist.", null);
#endif
                return false;
            }
        }

        /// <summary>
        /// オブジェクトが存在するかどうか.
        /// </summary>
        public bool IsExistObject(string id)
        {
            return objectDictionary.ContainsKey(id);
        }

        /// <summary>
        /// 全オブジェクト取得.
        /// </summary>
        public IEnumerable<T> GetAllObject()
        {
            foreach (KeyValuePair<string, T> pair in objectDictionary)
            {
                yield return pair.Value;
            }
        }

        /// <summary>
        /// 数取得.
        /// </summary>
        public int GetCount()
        {
            return objectDictionary.Count;
        }
    }
}
