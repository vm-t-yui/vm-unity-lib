/******************************************************************************/
/*!    \brief  キャプチャイベントを走らせるだけのアクタ.
*******************************************************************************/
#if !UNITY_EDITOR
using UnityEngine;

public sealed class CaptureActor : MonoBehaviour
{
    /// <summary>
    /// キャプチャ.
    /// </summary>
    public void CaptureScreenShot()
    {
// #if !DISABLE_SHARE_HELP
//         ShareHelper.Inst.CaptureScreenShot();
// #endif
    }
}
#endif
