/******************************************************************************/
/*!    \brief  transformをコピーする.
*******************************************************************************/

using UnityEngine;

public sealed class TransformTracker : MonoBehaviour 
{
    [SerializeField]
    Transform target = default;

    public void Update()
    {
        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}
