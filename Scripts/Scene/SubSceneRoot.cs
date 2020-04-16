/******************************************************************************/
/*!    \brief  サブシーンのルートオブジェクト.
*******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using UnityScene = UnityEngine.SceneManagement.Scene;

namespace VMUnityLib
{
    /// <summary>
    /// Sceneファイルのすべての親として存在させる。アクティブ時・非アクティブ時に子オブジェクトにイベントを流す.
    /// </summary>
    public class SubSceneRoot : MonoBehaviour
    {
        [SerializeField, SceneName,Tooltip("親シーン名")]
        string parentSceneName = default;
        public string ParentSceneName => parentSceneName;

        [SerializeField, SceneName,Tooltip("読まれているべきサブシーンリスト")]
        List<string> requireSubSceneNames = default;
        public List<string> RequireSubSceneNames => requireSubSceneNames;

        // Unityシーン情報
        public UnityScene UnityScene { get; private set; }

        /// <summary>
        /// 生成時.
        /// </summary>
        protected void Awake()
        {
            if (SceneManager.Instance != null)
            {
                // 自身がロード済だと自己申告.
                SceneManager.Instance.AddLoadedSubSceneRoot(this);
            }
        }

        public string GetSceneName()
        {
            return gameObject.scene.name;
        }
    }
}