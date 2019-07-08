/******************************************************************************/
/*!    \brief  シーンマネージャ.
*******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathologicalGames;
namespace VMUnityLib
{
    public delegate void AfterSceneControlDelegate();

    /// <summary>
    /// 子シーンは一度ロードされるとゲーム終了時までオンメモリ。以降は必要なときだけアクティブにする.
    /// </summary>
    public class SceneManager : MonoBehaviour
    {
	    public const string SCENE_ROOT_NAME_HEADER = "SceneRoot_";
        private const float DEBGUG_DUMMY_LOAD_TIME = 0.5f;
        private const float DUMMY_LOAD_TIME = 2.0f;

	    private SceneRoot	  	currentSceneRoot = null;						    // 現在アクティブなシーンルート.
	    private List<SceneRoot>	loadedSceneRootList = new List<SceneRoot>();	    // ロード済みのシーンルート.
        private Stack<string>   sceneHistory = new Stack<string>();                 // シーンの遷移ヒストリ.

	    private Stack<KeyValuePair<string, int>> sceneAnchor = new Stack<KeyValuePair<string, int>>();    // シーンのアンカー.

        private bool            isFadeWaiting = false;
        private CommonSceneUI   sceneUI = null;

        [SceneNameAttribute, SerializeField] private string firstSceneName;
	    [SceneNameAttribute, SerializeField] private string debugFirstSceneName;
        [SerializeField] private bool isDebug = true;

        public static SceneManager Instance { get; private set; }

        // シーンチェンジ時のフェードのパラメータ.
        public struct SceneChangeFadeParam
        {
            public float                            fadeOutTime;
            public float                            fadeInTime;
            public CmnFadeManager.FadeType          fadeType;
            public Color                            fadeColor;
            public LibBridgeInfo.LoadingType        loadingType;

            public SceneChangeFadeParam(float inFadeOutTime, float inFadeInTime, CmnFadeManager.FadeType inFadeType, Color inFadeColor, LibBridgeInfo.LoadingType inLoadingType)
            {
                fadeOutTime = inFadeOutTime;
                fadeInTime = inFadeInTime;
                fadeType = inFadeType;
                fadeColor = inFadeColor;
                loadingType = inLoadingType;
            }
        }

	    /// <summary>
	    /// Start this instance.
	    /// </summary>
	    void Start()
	    {
            StartCoroutine(SceneStartColutine());
        }
        IEnumerator SceneStartColutine()
        {
            sceneUI = CommonSceneUI.Inst;
            SceneChangeFadeParam noTimeFade = new SceneChangeFadeParam(0, 0, CmnFadeManager.FadeType.FADE_TIMEONLY, new Color(0, 0, 0, 0), LibBridgeInfo.LoadingType.COMMON);
            //　準備が整うまでシーン開始は遅延.
            yield return LibBridgeInfo.WaitForEndOfFrame;
            yield return LibBridgeInfo.WaitForEndOfFrame;
            if (!isDebug)
            {
                PushScene(firstSceneName, noTimeFade);
            }
            else
            {
                // デバッグの場合はデバッグシーンをアンカーとして埋め込んでおく.
                PushScene(debugFirstSceneName, noTimeFade, null, debugFirstSceneName);
            }
        }

        /// <summary>
        /// Awake this instance.
        /// </summary>
	    void Awake()
	    {
		    if(Instance == null)
		    {
			    Instance = this;
		    }
		    else
		    {
			    Destroy(this);
		    }
	    }

	    /*
	     * TODO
	     * 
	    ・特定のシーンをソフトリセット
	　    （StartとInitを呼ぶ。Awakeも呼びたい）
	
	    ・特定のシーンをハードリセット
	　    （Static以外の値や座標を強制リセット。ソフトより安定するが少し遅い）
	
	    ・LoadLevelを呼び出してシーンを綺麗にしてもらう
	　    （これで管理するのが一番楽）
	
	    ・特定のシーンを破棄してメモリを確保
	
	    ・メモリ不足した時に通知（ios/Androidのみ）
	　    （この時、アクティブでないキャッシュ済シーンを全て破棄してメモリを確保）
	
	    ・ゲームをポーズした時にアクティブでないキャッシュ済シーンを破棄してメモリを確保
	　    （バックグラウンドに回った時に落とされる対策）
	     */
	
        /// <summary>
        /// シーンプッシュ.
        /// </summary>
        /// <param name="SceneName">シーン名.</param>
        /// <param name="fadeTime">フェードパラメータ.</param>
        /// <param name="pushAnchor">シーンまでのアンカー.</param>
        public void PushScene(string SceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate = null, string pushAnchor = null)
	    {
            isFadeWaiting = true;
            CmnFadeManager.Inst.StartFadeOut(EndFadeOutCallBack, fadeParam.fadeOutTime, fadeParam.fadeType, fadeParam.fadeColor);
            StartCoroutine(PushSceneInternal(SceneName, fadeParam, afterSceneControlDelegate, pushAnchor));
        }
        private IEnumerator PushSceneInternal(string SceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate, string pushAnchor)
        {
            while (isFadeWaiting)
            {
                yield return null;
            }

            LoadingUIManager.Inst.ShowLoadingUI(fadeParam.loadingType);
            yield return null;  // 表示のために１フレ待つ.

            yield return StartCoroutine(ChangeSceneActivation(SceneName));

            sceneHistory.Push(SceneName);
            if (pushAnchor != null)
            {
                bool find = false;
                foreach (KeyValuePair<string, int> pair in sceneAnchor)
                {
                    if (pair.Key == pushAnchor)
                    {
                        find = true;
                        break;
                    }
                }
                if (find == false)
                {
                    sceneAnchor.Push(new KeyValuePair<string, int>(pushAnchor, sceneHistory.Count - 1));
                }
                else
                {
                    Logger.Error("same name anchor found");
                }
            }
            CleaneUpAfterChangeSceneActivation(fadeParam, afterSceneControlDelegate);
        }
    
	    /// <summary>
        /// シーン変更.
        /// </summary>
        /// <param name="SceneName">シーン名.</param>
        /// <param name="fadeTime">フェードパラメータ.</param>
        public void ChangeScene(string SceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate = null)
        {
            isFadeWaiting = true;
            CmnFadeManager.Inst.StartFadeOut(EndFadeOutCallBack, fadeParam.fadeOutTime, fadeParam.fadeType, fadeParam.fadeColor);
            StartCoroutine(ChangeSceneInternal(SceneName, fadeParam, afterSceneControlDelegate));
	    }
        private IEnumerator ChangeSceneInternal(string SceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate)
        {
            while (isFadeWaiting)
            {
                yield return null;
            }

            LoadingUIManager.Inst.ShowLoadingUI(fadeParam.loadingType);
            yield return null;  // 表示のために１フレ待つ.

            if (sceneHistory.Count > 0)
            {
                sceneHistory.Pop();
                sceneHistory.Push(SceneName);
                yield return StartCoroutine(ChangeSceneActivation(SceneName));
            }
            CleaneUpAfterChangeSceneActivation(fadeParam, afterSceneControlDelegate);
        }

	    /// <summary>
        /// シーンポップ.
        /// </summary>
        /// <param name="fadeTime">フェードパラメータ.</param>
        public void PopScene(SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate = null)
        {
            isFadeWaiting = true;
            CmnFadeManager.Inst.StartFadeOut(EndFadeOutCallBack, fadeParam.fadeOutTime, fadeParam.fadeType, fadeParam.fadeColor);
            StartCoroutine(PopSceneInternal(fadeParam, afterSceneControlDelegate));
	    }
        private IEnumerator PopSceneInternal(SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate)
        {
            while (isFadeWaiting)
            {
                yield return null;
            }

            LoadingUIManager.Inst.ShowLoadingUI(fadeParam.loadingType);
            yield return null;  // 表示のために１フレ待つ.

            if (sceneHistory.Count > 0)
            {
                sceneHistory.Pop();
                string SceneName = sceneHistory.Peek();
                yield return StartCoroutine(ChangeSceneActivation(SceneName));
            }
            CleaneUpAfterChangeSceneActivation(fadeParam, afterSceneControlDelegate);
        }

        /// <summary>
        /// 指定アンカーまでシーンポップ.
        /// </summary>
        /// <param name="SceneName">ポップ先のアンカー名.</param>
        /// <param name="fadeTime">フェードパラメータ.</param>
        public void PopSceneToAnchor(string anchorName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate = null)
        {
            isFadeWaiting = true;

            // アンカーが無ければ警告出して無視
            bool bFound = false;
            foreach (KeyValuePair<string, int> pair in sceneAnchor)
            {
                if (pair.Key == anchorName)
                {
                    CmnFadeManager.Inst.StartFadeOut(EndFadeOutCallBack, fadeParam.fadeOutTime, fadeParam.fadeType, fadeParam.fadeColor);
                    StartCoroutine(PopSceneToAnchorInternal(anchorName, fadeParam, afterSceneControlDelegate));
                    bFound = true;
                    break;
                }
            }
            if(bFound == false)
            {
                Logger.Warn("Anchor not found.:" + anchorName);
            }
        }
        private IEnumerator PopSceneToAnchorInternal(string anchorName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate)
        {
            while (isFadeWaiting)
            {
                yield return null;
            }

            LoadingUIManager.Inst.ShowLoadingUI(fadeParam.loadingType);
            yield return null;  // 表示のために１フレ待つ.

            // 最初に見つかった同名アンカーまでポップ.
            int popAnchorCount = -1;
            foreach (KeyValuePair<string, int> pair in sceneAnchor)
            {
                if (pair.Key == anchorName)
                {
                    popAnchorCount = pair.Value;
                    break;
                }
            }
            if (popAnchorCount >= 0)
            {
                while (sceneHistory.Count - 1 > popAnchorCount)
                {
                    sceneHistory.Pop();
                }
                string nextSceneName = sceneHistory.Peek();
                yield return StartCoroutine(ChangeSceneActivation(nextSceneName));
            }
            else
            {
                Logger.Error("invalid anchor name:" + anchorName);
            }
            CleaneUpAfterChangeSceneActivation(fadeParam, afterSceneControlDelegate);
        }


        /// <summary>
        /// 現在読まれているシーン以外をすべてアンロード.
        /// </summary>
        public void UnloadAllOtherScene()
        {
            // Sceneがすでにロードされているなら、そのシーンをアクティブにする.
            foreach (SceneRoot root in loadedSceneRootList) 
            {
                if(root != currentSceneRoot)
                {
#if UNITY_5_5_OR_NEWER
                    UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(root.GetSceneName());
#else
                    UnityEngine.SceneManagement.SceneManager.UnloadScene(root.GetSceneName());
#endif
                    GameObject.Destroy(root.gameObject);
                }
            }
            loadedSceneRootList = new List<SceneRoot> ();
            loadedSceneRootList.Add (currentSceneRoot);
        }

        /// <summary>
        /// シーンをすべてアンロード.
        /// </summary>
        public void DebugUnloadAllScene()
        {
            // Sceneがすでにロードされているなら、そのシーンをアクティブにする.
            foreach (SceneRoot root in loadedSceneRootList)
            {
#if UNITY_5_5_OR_NEWER
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(root.GetSceneName());
#else
                UnityEngine.SceneManagement.SceneManager.UnloadScene(root.GetSceneName());
#endif
                GameObject.Destroy(root.gameObject);
            }
            loadedSceneRootList = new List<SceneRoot>();
            sceneAnchor = new Stack<KeyValuePair<string, int>>();
        }

        /// <summary>
        /// フェードアウト終了時のコールバック.
        /// </summary>
        private void EndFadeOutCallBack()
        {
            isFadeWaiting = false;
        }

        /// <summary>
        /// フェードイン終了時のコールバック.
        /// </summary>
        private void EndFadeInCallBack()
        {
            // フェード終了メッセージを流す.
            if (currentSceneRoot)
            {
                currentSceneRoot.BroadcastMessage(CmnMonoBehaviour.FADE_END_NAME, SendMessageOptions.DontRequireReceiver);
            }
        }

	    /// <summary>
	    /// シーンのアクティブ状態を変更する.
	    /// </summary>
	    private IEnumerator ChangeSceneActivation(string SceneName)
	    {
		    // 現在のシーンをディアクティブにする.
		    if (currentSceneRoot) 
		    {
			    currentSceneRoot.SetSceneDeactive();
		    }

            // デバッグだったらダミー遅延.
            if (isDebug)
            {
                float startTime = Time.time;
                while (Time.time - startTime < DEBGUG_DUMMY_LOAD_TIME)
                {
                    yield return null;
                }
            }

            // 偽の読み込み時間分待機
            if(currentSceneRoot != null)
            {
                if (currentSceneRoot.GetSceneName() != "demo")
                {
                    yield return new WaitForSeconds(DUMMY_LOAD_TIME);
                }
            }

            // Sceneがすでにロードされているなら、そのシーンをアクティブにする.
            bool isLoaded = false;
		    foreach (SceneRoot root in loadedSceneRootList) 
		    {
			    if(root.GetSceneName() == SceneName)
			    {
				    root.SetSceneActive();
				    currentSceneRoot = root;
				    isLoaded = true;

				    // プールマネージャー初期化.
				    foreach(KeyValuePair<string, SpawnPool> pool in PoolManager.Pools)
				    {
					    pool.Value.DespawnAll();
				    }

				    break;
			    }
		    }
		
		    //ロードされていなかったらAddiveLoadする.
		    if(isLoaded == false)
		    {
                AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(SceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);

                async.allowSceneActivation = true;
                while(async.isDone == false || async.progress < 1.0f)
                {
                    yield return LibBridgeInfo.WaitForEndOfFrame;
                }

                GameObject sceneRootObj = GetLoadedSceneRoot(SCENE_ROOT_NAME_HEADER + SceneName);
                int waitCnt = 0;
                while (sceneRootObj == null && waitCnt < 120)
                {
                    ++waitCnt;
                    sceneRootObj = GetLoadedSceneRoot(SCENE_ROOT_NAME_HEADER + SceneName);
                    yield return LibBridgeInfo.WaitForEndOfFrame;
                }
                if (sceneRootObj == null)
                {
                    Debug.LogError("Scene root not found:" + SCENE_ROOT_NAME_HEADER + SceneName + " waited" + waitCnt + "frame.");
                    DumpLoadedSceneRoot();

                    UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(SceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
                    sceneRootObj = GetLoadedSceneRoot(SCENE_ROOT_NAME_HEADER + SceneName);
                    if (sceneRootObj == null)
                    {
                        Debug.LogError("no sync load but Scene root not found:" + SCENE_ROOT_NAME_HEADER + SceneName);
                        DumpLoadedSceneRoot();
                    }
                }
                sceneRootObj.transform.parent = gameObject.transform;
			    SceneRoot root = sceneRootObj.GetComponent<SceneRoot>();
			    currentSceneRoot = root;
                currentSceneRoot.SetSceneActive();
            }

            // unityシーンをアクティブにする.
            if (UnityEngine.SceneManagement.SceneManager.SetActiveScene(UnityEngine.SceneManagement.SceneManager.GetSceneByName(SceneName)) == false)
            {
                Debug.Log("active scene fail");
            }
        }

        /// <summary>
        /// ロード済シーンルートを追加する（SceneRootが自己申告）.
        /// </summary>
        public void AddLoadedSceneRoot(SceneRoot sceneRoot)
        {
            if(loadedSceneRootList.Contains(sceneRoot) == false)
            {
                loadedSceneRootList.Add(sceneRoot);
            }
        }

        /// <summary>
        /// シーン切り替え後のクリーンアップ（フェードイン等）.
        /// </summary>
        private void CleaneUpAfterChangeSceneActivation(SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate)
        {
            LoadingUIManager.Inst.HideLoadingUI(fadeParam.loadingType);
            CmnFadeManager.Inst.StartFadeIn(EndFadeInCallBack, fadeParam.fadeInTime, fadeParam.fadeType, fadeParam.fadeColor);
            sceneUI.ChangeCommonSceneUI(currentSceneRoot.SceneUiParam, currentSceneRoot.SceneBgKind);
            sceneUI.ChangeSceneTitleLabel(currentSceneRoot.SceneNameLocalizeID);
            if(afterSceneControlDelegate != null)
            {
                afterSceneControlDelegate();
            }
        }

        /// <summary>
        /// ロードが終了しているシーンルートを取得する.
        /// </summary>
        /// <returns></returns>
        private GameObject GetLoadedSceneRoot(string rootName)
        {
            foreach(SceneRoot root in loadedSceneRootList)
            {
                if(root.gameObject.name == rootName)
                {
                    return root.gameObject;
                }
            }
            return null;
        }

        /// <summary>
        /// ロード済シーンルートをダンプ.
        /// </summary>
        private void DumpLoadedSceneRoot()
        {
            Debug.Log("---- loaded scene ---");
            foreach (SceneRoot root in loadedSceneRootList)
            {
                Debug.Log(" " + root.gameObject.name);
            }
            Debug.Log("-------");
        }
    }
}