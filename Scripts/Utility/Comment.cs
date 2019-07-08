/******************************************************************************/
/*!    \brief  コメントを入れるだけのエディターオブジェクト.
*******************************************************************************/

using UnityEngine;

namespace VMUnityLib
{
    public sealed class Comment : MonoBehaviour
    {
#if UNITY_EDITOR
        public string comment;
#endif
    }
}
