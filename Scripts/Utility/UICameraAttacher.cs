using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 指定されたCanvasにUI用のCameraをアタッチする
/// </summary>
public class UICameraAttacher : MonoBehaviour
{
    [SerializeField] Canvas targetCanvas = default;
    private void Awake()
    {
        targetCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        targetCanvas.worldCamera = CommonUiRoot.Inst.UiCamera;
    }
}
