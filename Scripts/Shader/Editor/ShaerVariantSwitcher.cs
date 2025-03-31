using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

public class ShaderVariantSwitcher : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        BuildTarget target = report.summary.platform;
        ShaderVariantCollection variantCollection = null;

        // プラットフォームごとのパスを設定
        if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
        {
            variantCollection = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>("Assets/MyGameAssets/LibBridge/AutoShaderVariants.shadervariants");
        }
        else if (target == BuildTarget.Switch)
        {
            variantCollection = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>("Assets/MyGameAssets/LibBridge/AutoShaderVariantsSwitch.shadervariants");
        }

        if (variantCollection == null)
        {
            throw new BuildFailedException("[ShaderVariant] 対応するShader Variant Collectionが見つかりませんでした");
        }

        // 現在の GraphicsSettings アセットを取得
        SerializedObject graphicsSettings = new SerializedObject(GraphicsSettings.GetGraphicsSettings());
        SerializedProperty preloadedShaders = graphicsSettings.FindProperty("m_PreloadedShaders");

        preloadedShaders.ClearArray();
        preloadedShaders.InsertArrayElementAtIndex(0);
        preloadedShaders.GetArrayElementAtIndex(0).objectReferenceValue = variantCollection;

        graphicsSettings.ApplyModifiedProperties();

        Debug.Log($"[ShaderVariant] {target} 用のVariant Collectionを設定しました");
    }
}
