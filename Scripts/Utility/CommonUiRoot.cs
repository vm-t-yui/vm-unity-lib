/******************************************************************************/
/*!    \brief  共通UIのルート.
*******************************************************************************/

using UnityEngine;
using VMUnityLib;

public sealed class CommonUiRoot : MonoBehaviour
{
#if DEBUG
    [SerializeField]
    GameObject debugMenu;
#endif
    /// <summary>
    /// 自身の生成前に呼ばれる関数。生成するしない関係なしに呼ばれる.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        if (SceneManager.Instance == null)
        {
            var prefabName = "CommonUiRoot";
            Object obj = Resources.Load(prefabName);
            GameObject prefab = (GameObject)obj;
            if (prefab == null)
            {
                var stackTraceStr = StackTraceUtility.ExtractStackTrace();
                Debug.LogAssertion(prefabName + "のロードに失敗 obj:" + obj + "\n" + stackTraceStr);
            }
            var instantiated = Instantiate(prefab);
            DontDestroyOnLoad(instantiated);
        }
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
