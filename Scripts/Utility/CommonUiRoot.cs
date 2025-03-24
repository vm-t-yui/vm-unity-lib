using UnityEngine;
using VMUnityLib;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 共通UIのルート。ENABLE_AB_LOADでAssetBundleからロードする場合、該当ファイルをExcludeResources.jsonでビルド時除外することを推奨
/// </summary>
public sealed class CommonUiRoot : SingletonMonoBehaviour<CommonUiRoot>
{
    const string prefabName = "CommonUiRoot";
#if DEBUG
    [SerializeField]
    GameObject debugMenu = default;
#if UNITY_EDITOR
    const string prefabPath = "Assets/MyGameAssets/LibBridge/Prefabs/CommonUiRoot.prefab";
#endif
#endif
    /// <summary>
    /// 自身の生成前に呼ばれる関数。生成するしない関係なしに呼ばれる.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        GameObject prefab = null;
#if ENABLE_AB_LOAD && !UNITY_EDITOR
        // アセットバンドルからプレハブをロード
        var ab = AssetBundleLoader.LoadedAssetBundlesByName["commonprefab"];
        prefab = ab.LoadAsset<GameObject>(prefabName);
#else
        // Resourcesからロードする
        Object obj = Resources.Load(prefabName);
        prefab = (GameObject)obj;
#endif

        if (prefab == null)
        {
            Debug.LogAssertion(prefabName + "のロードに失敗 prefab:" + prefab);
#if UNITY_EDITOR
            prefab = (GameObject)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
#endif
        }
        else
        {
            Debug.Log("CommonUiRoot loaded.");
        }
        var instantiated = Instantiate(prefab);
        DontDestroyOnLoad(instantiated);
    }
    public static void ForceInitialize()
    {
        Object obj = Resources.Load(prefabName);
        GameObject prefab = (GameObject)obj;
        if (prefab == null)
        {
            Debug.LogAssertion(prefabName + "のロードに失敗 obj:" + obj);
        }
        var instantiated = Instantiate(prefab);
        DontDestroyOnLoad(instantiated);
    }

#if DEBUG
    /// <summary>
    /// デバッグメニュー呼び出し
    /// </summary>
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha0))
        {
            debugMenu.SetActive(!debugMenu.activeSelf);
        }
    }
#endif
}
