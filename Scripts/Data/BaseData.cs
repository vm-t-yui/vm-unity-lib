/******************************************************************************/
/*!    \brief  すべてのデータクラスの元になるクラス.
*******************************************************************************/

using UnityEngine;

namespace VMUnityLib
{
    public abstract class BaseData : ScriptableObject
    {
        public string       Id      { get { return name;       } private set { name = value;     } }

        /// <summary>
        /// ID取得.
        /// </summary>
        public string GetId()
        {
            return Id;
        }
    }
}
