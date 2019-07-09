/******************************************************************************/
/*!    \brief  スロー状態を管理するアクタ.
*******************************************************************************/

using UnityEngine;

public sealed class SlowActor : MonoBehaviour 
{
    float beforeSlow;

    /// <summary>
    ///  スロー開始.
    /// </summary>
    private void SlowStart(float t)
    {
        beforeSlow = Time.timeScale;
        Time.timeScale = t;
    }

    /// <summary>
    /// スロー終了.
    /// </summary>
    private void SlowEnd()
    {
        Time.timeScale = beforeSlow;
    }
}
