/******************************************************************************/
/*!    \brief  シーンチェンジ用コンポーネント.
*******************************************************************************/

using UnityEngine;
namespace VMUnityLib
{
    public class SceneChanger : MonoBehaviour
    {
        public CmnFadeManager.FadeType  FadeType = default;         // フェードのタイプ
        public Color                    FadeColor = Color.white;    // フェードの色

        /// <summary>
        /// シーンの変更
        /// </summary>
        public void ChangeScene(SceneName.SceneNameKind sceneNameKind)
        {
            SceneManager.SceneChangeFadeParam param = LibBridgeInfo.DefaultSceneChangeFadeParam;
            param.fadeType = FadeType;
            param.fadeColor = FadeColor;
            string sceneName = SceneName.SceneNameTable[sceneNameKind];
            SceneManager.Instance.ChangeScene(sceneName, param);
        }

        /// <summary>
        /// シーンのプッシュ
        /// </summary>
        public void PushScene(SceneName.SceneNameKind sceneNameKind)
        {
            SceneManager.SceneChangeFadeParam param = LibBridgeInfo.DefaultSceneChangeFadeParam;
            param.fadeType = FadeType;
            param.fadeColor = FadeColor;
            string sceneName = SceneName.SceneNameTable[sceneNameKind];
            SceneManager.Instance.PushScene(sceneName, param);
        }

        /// <summary>
        /// シーンのポップ
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