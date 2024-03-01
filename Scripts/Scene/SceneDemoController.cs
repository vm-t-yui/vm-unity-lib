/******************************************************************************/
/*!    \brief  デモシーンのコントローラー.
*******************************************************************************/

using UnityEngine;
using System.Collections;
using VMUnityLib;

public sealed class SceneDemoController : MonoBehaviour 
{
    [SerializeField]
    private Color fadeColor = default;

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
        var param = LibBridgeInfo.DefaultSceneChangeFadeParam;
        param.fadeColor = fadeColor;
        param.fadeType = CmnFadeManager.FadeType.FADE_COLOR;
        SceneManager.Instance.PushScene(SceneName.title, param);
    }
}
