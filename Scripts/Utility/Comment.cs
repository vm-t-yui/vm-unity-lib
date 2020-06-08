/******************************************************************************/
/*!    \brief  コメントを入れるだけのエディターオブジェクト.
*******************************************************************************/

using UnityEngine;

namespace VMUnityLib
{
    public sealed class Comment : MonoBehaviour
    {
#if UNITY_EDITOR
        [Multiline]
        public string comment;
#endif
    }
}
