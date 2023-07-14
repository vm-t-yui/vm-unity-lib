/******************************************************************************/
/*!    \brief  デモシーンのコントローラー.
*******************************************************************************/

using System.Collections;
using UnityEngine;
using VMUnityLib;

public sealed class SceneDemoController : MonoBehaviour
{
    /// <summary>
    /// 指定時間待ってからシーンチェンジ.
    /// </summary>
    public void ChangeScene(float time)
    {
        StartCoroutine(ChangeSceneCoroutine(time));
    }
    IEnumerator ChangeSceneCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        SceneManager.Instance.PushScene(SceneName.title, LibBridgeInfo.DefaultSceneChangeFadeParam);

    }
}
