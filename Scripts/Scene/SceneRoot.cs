/******************************************************************************/
/*!    \brief  シーンのルートオブジェクト.
*******************************************************************************/

using UnityEngine;
using System.Collections;

namespace VMUnityLib
{
    /// <summary>
    /// Sceneファイルのすべての親として存在させる。アクティブ時・非アクティブ時に子オブジェクトにイベントを流す.
    /// </summary>
    public class SceneRoot : MonoBehaviour 
    {
        // シーンUIとして何を表示するか.
        [SerializeField]
        private CommonSceneUI.CommonSceneUIParam sceneUiParam;
        public CommonSceneUI.CommonSceneUIParam SceneUiParam { get { return sceneUiParam; } }

        // シーン背景の種別.
        [SerializeField]
        private UISceneBG.SceneBgKind sceneBgKind;
        public UISceneBG.SceneBgKind SceneBgKind { get { return sceneBgKind; } }

        // シーン名表示用のローカライズID.
        [SerializeField]
        private string sceneNameLocalizeID;
        public string SceneNameLocalizeID { get { return sceneNameLocalizeID; } }

        private bool isDebug = false;

        /// <summary>
        /// 生成時.
        /// </summary>
        protected void Awake()
        {
            if (SceneManager.Instance != null)
            {
                // 自身がロード済だと自己申告.
                SceneManager.Instance.AddLoadedSceneRoot(this);
            }
            else
            {
                // デバッグ起動であることを通知.
                isDebug = true;
            }
        }

        /// <summary>
        /// スタート時.
        /// </summary>
        public void Start()
        {
            // デバッグ起動（シーン直接開始）なら初期化イベント等を呼ぶ.
            if(isDebug)
            {
                StartCoroutine(DebugInitCoroutine());
            }
        }
        IEnumerator DebugInitCoroutine()
        {
            yield return new WaitForEndOfFrame();
            gameObject.BroadcastMessage(CmnMonoBehaviour.INIT_SCENCE_CHANGE_NAME, 0, SendMessageOptions.DontRequireReceiver);
            yield return new WaitForEndOfFrame();
            gameObject.BroadcastMessage(CmnMonoBehaviour.FADE_END_NAME, 0, SendMessageOptions.DontRequireReceiver);
        }

        public string GetSceneName()
        {
            return gameObject.name.Remove(0, SceneManager.SCENE_ROOT_NAME_HEADER.Length);
        }

        /// <summary>
        /// Sets the scene active.
        /// </summary>
        public void SetSceneActive()
        {
            gameObject.SetActive (true);
            gameObject.BroadcastMessage (CmnMonoBehaviour.INIT_SCENCE_CHANGE_NAME, 0, SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Sets the scene deactive.
        /// </summary>
        public void SetSceneDeactive()
        {
            gameObject.BroadcastMessage(CmnMonoBehaviour.SCENCE_DEACTIVE_NAME, 0, SendMessageOptions.DontRequireReceiver);
            gameObject.SetActive (false);
        }
    }
}