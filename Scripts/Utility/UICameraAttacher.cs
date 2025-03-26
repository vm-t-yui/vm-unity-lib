using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 指定されたCanvasにUI用のCameraをアタッチする
/// </summary>
public class UICameraAttacher : MonoBehaviour
{
    [SerializeField] Canvas targetCanvas = default;
    [SerializeField] Camera targetCamera = default;
    private void Awake()
    {
        if (targetCanvas)
        {
            targetCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            targetCanvas.worldCamera = CommonUiRoot.Inst.UiCamera;
        }
        if(targetCamera)
        {
            // プレイヤーのカメラにUIカメラをアタッチ
            var baseCameraData = targetCamera.GetUniversalAdditionalCameraData();
            var overlayCameraData = CommonUiRoot.Inst.UiCamera.GetUniversalAdditionalCameraData();
            if (!baseCameraData.cameraStack.Contains(CommonUiRoot.Inst.UiCamera))
            {
                baseCameraData.cameraStack.Add(CommonUiRoot.Inst.UiCamera);
            }
            if (overlayCameraData.renderType != CameraRenderType.Overlay)
            {
                Debug.LogWarning("[CameraStackDebugger] Overlay カメラが Overlay タイプではありません。");
            }
            if (baseCameraData.cameraStack.Contains(CommonUiRoot.Inst.UiCamera))
            {
                Debug.Log("[CameraStackDebugger] UIカメラは Base カメラに正しくスタックされています。");
            }
            else
            {
                Debug.LogError("[CameraStackDebugger] UIカメラが Base カメラにスタックされていません。");
            }
        }
    }
}
