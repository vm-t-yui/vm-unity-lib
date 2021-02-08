/******************************************************************************/
/*!    \brief  シーンのルートオブジェクト.
*******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace VMUnityLib
{
    /// <summary>
    /// Sceneファイルのすべての親として存在させる。アクティブ時・非アクティブ時に子オブジェクトにイベントを流す.
    /// </summary>
    public class SceneRoot : MonoBehaviour 
    {
        [SerializeField, Tooltip("シーンUIとして何を表示するか")]
        CommonSceneUI.CommonSceneUIParam sceneUiParam = default;
        public CommonSceneUI.CommonSceneUIParam SceneUiParam => sceneUiParam;

        [SerializeField, Tooltip("シーン背景の種別")]
        UISceneBG.SceneBgKind sceneBgKind = default;
        public UISceneBG.SceneBgKind SceneBgKind => sceneBgKind;

        [SerializeField, Tooltip("シーン名表示用のローカライズID")]
        string sceneNameLocalizeID = default;
        public string SceneNameLocalizeID => sceneNameLocalizeID;
        
        [SerializeField, Tooltip("サブシーンを持っているかどうか")]
        bool hasSubScene = default;
        public bool HasSubScene => hasSubScene;

        [SerializeField, SceneName, Tooltip("サブシーン名リスト")]
        string[] subSceneList = default;
        public string[] SubSceneList => subSceneList;

        [SerializeField, SceneName, Tooltip("初回にロードするサブシーン")]
        string firstSubSceneName = default;
        public string FirstSubSceneName => firstSubSceneName;

        // Unityシーン情報
        public UnityScene UnityScene { get; private set; }

        bool isDebug = false;
        string sceneNameCache = default;

        /// <summary>
        /// 生成時.
        /// </summary>
        protected void Awake()
        {
            sceneNameCache = gameObject.scene.name;
            // Awake呼ばれるのは初回ロードのアクティブ状態の時なので
            UnityScene = UnitySceneManager.GetActiveScene();

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
            while(SceneManager.Instance == null)
            {
                // 自身がロード済だと自己申告.
                SceneManager.Instance.AddLoadedSceneRoot(this);
            }
        }

        public string GetSceneName()
        {
            return sceneNameCache;
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