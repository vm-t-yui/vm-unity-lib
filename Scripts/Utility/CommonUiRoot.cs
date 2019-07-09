/******************************************************************************/
/*!    \brief  共通UIのルート.
*******************************************************************************/

using UnityEngine;
using VMUnityLib;

public sealed class CommonUiRoot : MonoBehaviour
{
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
            GameObject.Instantiate(commonUiRootPrefab);
        }
    }
}
