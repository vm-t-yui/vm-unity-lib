using UnityEngine;

/// <summary>
/// Transformの拡張クラス
/// </summary>
public static class TransformExtension
{
    /// <summary>
    /// position、rotation、scaleをリセットする
    /// </summary>
    public static void Reset(this Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

}