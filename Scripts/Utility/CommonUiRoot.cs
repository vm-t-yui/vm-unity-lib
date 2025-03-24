/******************************************************************************/
/*!    \brief  共通UIのルート.
*******************************************************************************/

using UnityEngine;
using VMUnityLib;
#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class CommonUiRoot : SingletonMonoBehaviour<CommonUiRoot>
{
    const string prefabName = "CommonUiRoot";
#if DEBUG
    [SerializeField]
    GameObject debugMenu = default;
#endif
    /// <summary>
    /// 自身の生成前に呼ばれる関数。生成するしない関係なしに呼ばれる.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        // アセットバンドルからプレハブをロード
        var ab = AssetBundleLoader.LoadedAssetBundlesByName["commonprefab"];
        GameObject prefab = ab.LoadAsset<GameObject>(prefabName);

        if (prefab == null)
        {
            Debug.LogAssertion(prefabName + "のロードに失敗 prefab:" + prefab);
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
