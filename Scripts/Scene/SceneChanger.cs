/******************************************************************************/
/*!    \brief  シーンチェンジ用コンポーネント.
*******************************************************************************/

using UnityEngine;
namespace VMUnityLib
{
    public class SceneChanger : MonoBehaviour
    {
        public CmnFadeManager.FadeType  FadeType;
        public Color                    FadeColor = Color.white;

        /// <summary>
        /// Changes the scene.
        /// </summary>
        public void ChangeScene(string sceneName)
        {
            SceneManager.SceneChangeFadeParam param = LibBridgeInfo.DefaultSceneChangeFadeParam;
            param.fadeType = FadeType;
            param.fadeColor = FadeColor;
            SceneManager.Instance.ChangeScene(sceneName, param);
        }

        /// <summary>
        /// Push the scene.
        /// </summary>
        public void PushScene(string sceneName)
        {
            SceneManager.SceneChangeFadeParam param = LibBridgeInfo.DefaultSceneChangeFadeParam;
            param.fadeType = FadeType;
            param.fadeColor = FadeColor;
            SceneManager.Instance.PushScene(sceneName, param);
        }

        /// <summary>
        /// Pop the scene.
        /// </summary>
        public void PopScene()
        {
            SceneManager.SceneChangeFadeParam param = LibBridgeInfo.DefaultSceneChangeFadeParam;
            param.fadeType = FadeType;
            param.fadeColor = FadeColor;
            SceneManager.Instance.PopScene(param);
        }
    }
}