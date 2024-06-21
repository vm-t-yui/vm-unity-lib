using UnityEngine;
namespace VMUnityLib
{
    /// <summary>
    /// シーン変更用コンポーネント
    /// </summary>
    public class SceneChanger : MonoBehaviour
    {
        [SceneName]
        public string SceneName;

        public CmnFadeManager.FadeType  FadeType;
        public Color                    FadeColor = Color.white;
        public bool                     ChangeSceneOnEnable = false;    // オブジェクト有効化時にシーンチェンジするか
        public float                    OverrideFadeOutTime = -1.0f;    // 上書きするフェードアウト時間（-1で無効）
        public float                    OverrideFadeInTime = -1.0f;     // 上書きするフェードイン時間（-1で無効）

        /// <summary>
        /// 有効化時
        /// </summary>
        void OnEnable()
        {
            if(ChangeSceneOnEnable)
            {
                ChangeScene();
            }
        }

        /// <summary>
        /// Changes the scene.
        /// </summary>
        public void ChangeScene()
        {
            SceneManager.SceneChangeFadeParam param = LibBridgeInfo.DefaultSceneChangeFadeParam;
            param.fadeType = FadeType;
            param.fadeColor = FadeColor;
            if(OverrideFadeOutTime >= 0) param.fadeInTime = OverrideFadeOutTime;
            if(OverrideFadeInTime >= 0) param.fadeInTime = OverrideFadeInTime;
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
            if (OverrideFadeOutTime >= 0) param.fadeInTime = OverrideFadeOutTime;
            if (OverrideFadeInTime >= 0) param.fadeInTime = OverrideFadeInTime;
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
            if (OverrideFadeOutTime >= 0) param.fadeInTime = OverrideFadeOutTime;
            if (OverrideFadeInTime >= 0) param.fadeInTime = OverrideFadeInTime;
            SceneManager.Instance.PopScene(param);
        }
    }
}