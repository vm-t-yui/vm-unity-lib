#if UNITY_EDITOR
using UnityEditor;

/// <summary>
///  アニメーションのインポート後に走る設定の実装部.
/// </summary>
public class AnimationAutoSettings
{
	public static void OnAnimationProcess(ModelImporterClipAnimation clip)
	{
		clip.lockRootHeightY = true;
		clip.lockRootPositionXZ = true;
		clip.lockRootRotation = true;
		clip.keepOriginalOrientation = true;
		clip.keepOriginalPositionXZ = true;
		clip.keepOriginalPositionY = true;
	}
}
#endif