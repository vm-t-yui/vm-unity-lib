/******************************************************************************/
/*!    \brief  transformをコピーする.
*******************************************************************************/

using UnityEngine;

public sealed class TransformTracker : MonoBehaviour 
{
    [SerializeField]
    Transform target;

    public void Update()
    {
        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}
