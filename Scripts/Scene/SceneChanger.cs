/******************************************************************************/
/*!    \brief  シーンチェンジ用コンポーネント.
*******************************************************************************/

using UnityEngine;
namespace VMUnityLib
{
    public class SceneChanger : MonoBehaviour
    {
        [SceneNameAttribute]
        public string SceneName = default;        // シーンの名前

        public CmnFadeManager.FadeType  FadeType = default;         // フェードのタイプ
        public Color                    FadeColor = Color.white;    // フェードの色

        /// <summary>
        /// シーンの変更
        /// </summary>
        public void ChangeScene()
        {
            SceneManager.SceneChangeFadeParam param = LibBridgeInfo.DefaultSceneChangeFadeParam;
            param.fadeType = FadeType;
            param.fadeColor = FadeColor;
            SceneManager.Instance.ChangeScene(SceneName, param);
        }

        /// <summary>
        /// シーンのプッシュ
        /// </summary>
        public void PushScene()
        {
            SceneManager.SceneChangeFadeParam param = LibBridgeInfo.DefaultSceneChangeFadeParam;
            param.fadeType = FadeType;
            param.fadeColor = FadeColor;
            SceneManager.Instance.PushScene(SceneName, param);
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