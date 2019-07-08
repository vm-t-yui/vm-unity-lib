/******************************************************************************/
/*!    \brief  nendのネイティブのみの表示
*******************************************************************************/

using UnityEngine;
using NendUnityPlugin.AD.Native;

public class MyNendNative : MonoBehaviour
{
    [SerializeField]
    NendAdNativeView nativeView;
    [SerializeField]
    NendAdNative     native;

    /// <summary>
    /// 開始時処理
    /// </summary>
    private void Start()
    {
        native.LoadAd();
    }

    /// <summary>
    /// 表示非表示の切り替え
    /// </summary>
    /// <param name="isShow"></param>
    public void Show(bool isShow)
    {
#if !UNITY_EDITOR
        nativeView.gameObject.SetActive(isShow);
#endif
    }
}
