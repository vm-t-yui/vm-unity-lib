/******************************************************************************/
/*!    \brief  コリジョンに関するアクタ.
*******************************************************************************/

using UnityEngine;

public sealed class CollisionActor : MonoBehaviour 
{
    [SerializeField]
    SphereCollider sphere = default;

    /// <summary>
    /// スフィアコライダの半径を変える.
    /// </summary>
    public void ChangeSphereRad(float radius)
    {
        sphere.radius = radius;
    }
}
