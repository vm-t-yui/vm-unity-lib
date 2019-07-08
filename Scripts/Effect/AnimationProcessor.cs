#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// アニメーションのインポート設定のポストプロセス.
/// </summary>
public class AnimationProcessor : AssetPostprocessor
{
	static void SetAnimationImporterSettings(ModelImporter importer)
	{
		var clips = importer.clipAnimations;

		if (clips.Length == 0) clips = importer.defaultClipAnimations;

		foreach (var clip in clips)
		{
			AnimationAutoSettings.OnAnimationProcess(clip);
		}

		importer.clipAnimations = clips;
	}

	void OnPreprocessAnimation()
	{
		SetAnimationImporterSettings(assetImporter as ModelImporter);
	}

	[MenuItem("Assets/Set Animation Options", true)]
	static bool SetAnimationOptionsValidate()
	{
		return Selection.GetFiltered(typeof(GameObject), SelectionMode.Assets).Length > 0;
	}

	[MenuItem("Assets/Set Animation Options")]
	static void SetAnimationOptions()
	{
		var filtered = Selection.GetFiltered(typeof(GameObject), SelectionMode.Assets);
		foreach (var go in filtered)
		{
			var path = AssetDatabase.GetAssetPath(go);
			var importer = AssetImporter.GetAtPath(path);
			SetAnimationImporterSettings(importer as ModelImporter);
			AssetDatabase.ImportAsset(path);
		}
		Selection.activeObject = null;
	}
}
#endif