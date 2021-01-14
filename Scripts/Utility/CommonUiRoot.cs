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
            // Unityがロード失敗するのが急に起こるようになったので調暫定処理
            GameObject commonUiRootPrefab = null;
            int loadCnt = 0;
            while (commonUiRootPrefab == null && loadCnt < 100)
            {
                commonUiRootPrefab = Resources.Load<GameObject>("CommonUiRoot");
                if (commonUiRootPrefab == null)
                {
                    System.Threading.Thread.Sleep(10);
                    ++loadCnt;
                }
            }
            if (loadCnt != 0)
            {
                Debug.LogError("commonUiRootPrefabのロードに" + loadCnt + "回失敗");
            }
            var obj = Instantiate(commonUiRootPrefab);
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
