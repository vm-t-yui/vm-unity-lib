/******************************************************************************/
/*!    \brief  自殺イベントだけを持つビヘイビア.
*******************************************************************************/

using UnityEngine;

public sealed class SelfDestroy : MonoBehaviour 
{
    /// <summary>
    /// 自身をDestroyする.
    /// </summary>
   public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
