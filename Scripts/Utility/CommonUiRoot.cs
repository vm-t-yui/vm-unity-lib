/******************************************************************************/
/*!    \brief  共通UIのルート.
*******************************************************************************/

using UnityEngine;
using VMUnityLib;
#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class CommonUiRoot : MonoBehaviour
{
    const string prefabName = "CommonUiRoot";
#if DEBUG
    [SerializeField]
    GameObject debugMenu = default;
#if UNITY_EDITOR
    const string prefabPath = "Assets/MyGameAssets/LibBridge/Resources/"+ prefabName+ ".prefab";
#endif
#endif
    /// <summary>
    /// 自身の生成前に呼ばれる関数。生成するしない関係なしに呼ばれる.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        Object obj = Resources.Load(prefabName);
        GameObject prefab = (GameObject)obj;
        if (prefab == null)
        {
            Debug.LogAssertion(prefabName + "のロードに失敗 obj:" + obj);
#if UNITY_EDITOR
            prefab = (GameObject)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
#endif
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
