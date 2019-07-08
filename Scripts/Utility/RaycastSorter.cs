/******************************************************************************/
/*!    \brief  レイをソートする.
*******************************************************************************/

using UnityEngine;
using System.Collections;
namespace VMUnityLib
{
    public class RaycastSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            RaycastHit raycastHitA = (RaycastHit)x;
            RaycastHit raycastHitB = (RaycastHit)y;
            return raycastHitA.distance.CompareTo(raycastHitB.distance);
        }
    }
}