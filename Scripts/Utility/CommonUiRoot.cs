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
            GameObject commonUiRootPrefab = Resources.Load<GameObject>("CommonUiRoot");
            if (commonUiRootPrefab == null)
            {
                Debug.LogError("commonUiRootPrefabのロードに失敗");
            }
            var obj = GameObject.Instantiate(commonUiRootPrefab);
            DontDestroyOnLoad(obj);
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
