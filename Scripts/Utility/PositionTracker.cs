/******************************************************************************/
/*!    \brief  xとzのポジションを追跡する.
*******************************************************************************/

using UnityEngine;

public sealed class PositionTracker : MonoBehaviour 
{
    [SerializeField]
    Transform target;

    public bool x = true;
    public bool y = false;
    public bool z = true;

    public float lerpTrackingSpeed = 1.0f;

    /// <summary>
    /// ターゲット設定.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// 更新.
    /// </summary>
    void FixedUpdate()
    {
        if (target != null)
        {
            Vector3 aimPos = new Vector3(
                x ? target.position.x : transform.position.x,
                y ? target.position.y : transform.position.y,
                z ? target.position.z : transform.position.z);

            transform.position = Vector3.Lerp(transform.position, aimPos, lerpTrackingSpeed);
        }
    }
}
