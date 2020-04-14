/******************************************************************************/
/*!    \brief  シーンチェンジ用コンポーネント.
*******************************************************************************/

using UnityEngine;
namespace VMUnityLib
{
    public class SceneChanger : MonoBehaviour
    {
        [SceneName]
        public string SceneName;

        public CmnFadeManager.FadeType  FadeType;
        public Color                    FadeColor = Color.white;

        /// <summary>
        /// Changes the scene.
        /// </summary>
        public void ChangeScene()
        {
            SceneManager.SceneChangeFadeParam param = LibBridgeInfo.DefaultSceneChangeFadeParam;
            param.fadeType = FadeType;
            param.fadeColor = FadeColor;
            SceneManager.Instance.ChangeScene(SceneName, param);
        }

        /// <summary>
        /// Push the scene.
        /// </summary>
        public void PushScene()
        {
            SceneManager.SceneChangeFadeParam param = LibBridgeInfo.DefaultSceneChangeFadeParam;
            param.fadeType = FadeType;
            param.fadeColor = FadeColor;
            SceneManager.Instance.PushScene(SceneName, param);
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