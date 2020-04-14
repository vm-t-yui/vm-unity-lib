/******************************************************************************/
/*!    \brief  シーンマネージャ.
*******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
#if USE_POOL_MANAGER
using PathologicalGames;
#endif
namespace VMUnityLib
{
    public delegate void AfterSceneControlDelegate();

    /// <summary>
    /// 子シーンは一度ロードされるとゲーム終了時までオンメモリ。以降は必要なときだけアクティブにする.
    /// </summary>
    public class SceneManager : MonoBehaviour
    {
        public const string SCENE_ROOT_NAME_HEADER = "SceneRoot_";
        const float DEBGUG_DUMMY_LOAD_TIME = 0.5f;
        const float DUMMY_LOAD_TIME = 2.0f;

        SceneRoot           currentSceneRoot = null;                            // 現在アクティブなシーンルート.
        List<SceneRoot>     loadedSceneRootList = new List<SceneRoot>();        // ロード済みのシーンルート.
        Stack<string>       sceneHistory = new Stack<string>();                 // シーンの遷移ヒストリ.
        bool                isFadeWaiting = false;
        CommonSceneUI       sceneUI = null;
        bool                isDirectBoot;                                       // 直接起動かどうか

        [SceneName, SerializeField] string firstSceneName = default;
        [SceneName, SerializeField] string debugFirstSceneName = default;
        [SerializeField] bool isDebug = true;
        public static SceneManager Instance { get; set; }

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

            // 直接シーン起動でない場合のみデフォルトシーンの読み込みを開始
            if (!isDirectBoot)
            {
                if (!isDebug)
                {
                    PushScene(firstSceneName, noTimeFade);
                }
                else
                {
                    // デバッグの場合はデバッグシーンをアンカーとして埋め込んでおく.
                    PushScene(debugFirstSceneName, noTimeFade);
                }
            }
        }

        /// <summary>
        /// Awake this instance.
        /// </summary>
        void Awake()
        {
            // アクティブなシーン名が"root"でない場合は直接起動フラグをたてる
            if (UnitySceneManager.GetActiveScene().name != "root")
            {
                isDirectBoot = true;
            }
            if (Instance == null)
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
        /// <param name="sceneName">シーン名.</param>
        /// <param name="fadeTime">フェードパラメータ.</param>
        /// <param name="pushAnchor">シーンまでのアンカー.</param>
        public void PushScene(string sceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate = null)
        {
            isFadeWaiting = true;
            CmnFadeManager.Inst.StartFadeOut(EndFadeOutCallBack, fadeParam.fadeOutTime, fadeParam.fadeType, fadeParam.fadeColor);
            StartCoroutine(PushSceneInternal(sceneName, fadeParam, afterSceneControlDelegate));
        }
        IEnumerator PushSceneInternal(string sceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate)
        {
            while (isFadeWaiting)
            {
                yield return null;
            }

            LoadingUIManager.Inst.ShowLoadingUI(fadeParam.loadingType);
            yield return null;  // 表示のために１フレ待つ.

            yield return StartCoroutine(ChangeSceneActivation(sceneName));

            sceneHistory.Push(sceneName);
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
        IEnumerator ChangeSceneInternal(string SceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate)
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
        IEnumerator PopSceneInternal(SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate)
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
        /// 指定シーンまでシーンポップ.
        /// </summary>
        /// <param name="sceneName">ポップ先のシーン名.</param>
        /// <param name="fadeTime">フェードパラメータ.</param>
        public void PopSceneTo(string sceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate = null)
        {
            isFadeWaiting = true;

            // アンカーが無ければ警告出して無視
            bool bFound = false;
            foreach (var item in sceneHistory)
            {
                if (item == sceneName)
                {
                    CmnFadeManager.Inst.StartFadeOut(EndFadeOutCallBack, fadeParam.fadeOutTime, fadeParam.fadeType, fadeParam.fadeColor);
                    StartCoroutine(PopSceneToInternal(sceneName, fadeParam, afterSceneControlDelegate));
                    bFound = true;
                    break;
                }
            }
            if(bFound == false)
            {
                Debug.LogWarning("Anchor not found.:" + sceneName);
            }
        }
        IEnumerator PopSceneToInternal(string sceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate)
        {
            while (isFadeWaiting)
            {
                yield return null;
            }

            LoadingUIManager.Inst.ShowLoadingUI(fadeParam.loadingType);
            yield return null;  // 表示のために１フレ待つ.

            // 最初に見つかった同名シーンまでポップ.
            Stack<string> sceneHistoryCopy = new Stack<string>(sceneHistory);
            sceneHistoryCopy.Pop(); // 現在のシーンが最初に入っているので削除
            int popCount = 0;
            while (sceneHistoryCopy.Count > 0)
            {
                ++popCount;
                if(sceneName != sceneHistoryCopy.Peek())
                {
                    sceneHistoryCopy.Pop();
                }
                else
                {
                    break;
                }
            }
            if (popCount >= sceneHistory.Count)
            {
                Debug.LogError("invalid scene name:" + sceneName);
            }
            else
            {
                for (int i = 0; i < popCount; i++)
                {
                    sceneHistory.Pop();
                }
                string nextSceneName = sceneHistory.Peek();
                yield return StartCoroutine(ChangeSceneActivation(nextSceneName));
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
                    UnitySceneManager.UnloadSceneAsync(root.GetSceneName());
#else
                    UnitySceneManager.UnloadScene(root.GetSceneName());
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
                UnitySceneManager.UnloadSceneAsync(root.GetSceneName());
#else
                UnitySceneManager.UnloadScene(root.GetSceneName());
#endif
                GameObject.Destroy(root.gameObject);
            }
            loadedSceneRootList = new List<SceneRoot>();
        }

        /// <summary>
        /// フェードアウト終了時のコールバック.
        /// </summary>
        void EndFadeOutCallBack()
        {
            isFadeWaiting = false;
        }

        /// <summary>
        /// フェードイン終了時のコールバック.
        /// </summary>
        void EndFadeInCallBack()
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
        IEnumerator ChangeSceneActivation(string SceneName)
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
#if USE_POOL_MANAGER
                    // プールマネージャー初期化.
                    foreach(KeyValuePair<string, SpawnPool> pool in PoolManager.Pools)
                    {
                        pool.Value.DespawnAll();
                    }
#endif

                    break;
                }
            }
        
            //ロードされていなかったらAddiveLoadする.
            if(isLoaded == false)
            {
                AsyncOperation async = UnitySceneManager.LoadSceneAsync(SceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);

                async.allowSceneActivation = true;
                while(async.isDone == false || async.progress < 1.0f)
                {
                    yield return LibBridgeInfo.WaitForEndOfFrame;
                }

                GameObject sceneRootObj = GetLoadedSceneRoot(SCENE_ROOT_NAME_HEADER + SceneName);

                // 120フレーム待ってもアクティブなルートが取れない場合は原因不明のエラーとしてassert
                int waitCnt = 0;
                while (sceneRootObj == null && waitCnt < 120)
                {
                    ++waitCnt;
                    sceneRootObj = GetLoadedSceneRoot(SCENE_ROOT_NAME_HEADER + SceneName);
                    yield return LibBridgeInfo.WaitForEndOfFrame;
                }
                if (sceneRootObj == null)
                {
                    Debug.LogAssertion("Scene root not found:" + SCENE_ROOT_NAME_HEADER + SceneName + " waited" + waitCnt + "frame.");
                    DumpLoadedSceneRoot();

                    UnitySceneManager.LoadSceneAsync(SceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
                    sceneRootObj = GetLoadedSceneRoot(SCENE_ROOT_NAME_HEADER + SceneName);
                    if (sceneRootObj == null)
                    {
                        Debug.LogAssertion("no sync load but Scene root not found:" + SCENE_ROOT_NAME_HEADER + SceneName);
                        DumpLoadedSceneRoot();
                    }
                }

                // UNITYのバージョンアップにともない、マルチシーン機能が実装されたため親子関係による変更は廃止
                //sceneRootObj.transform.parent = gameObject.transform;
                SceneRoot root = sceneRootObj.GetComponent<SceneRoot>();
                currentSceneRoot = root;
                currentSceneRoot.SetSceneActive();
            }

            // unityシーンをアクティブにする.
            if (UnitySceneManager.SetActiveScene(UnitySceneManager.GetSceneByName(SceneName)) == false)
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

            // 直接起動の場合は現在のシーン更新してシーンヒストリーに入れる
            if(currentSceneRoot == null && isDirectBoot)
            {
                currentSceneRoot = sceneRoot;
                sceneHistory.Push(currentSceneRoot.GetSceneName());
            }
        }

        /// <summary>
        /// シーン切り替え後のクリーンアップ（フェードイン等）.
        /// </summary>
        void CleaneUpAfterChangeSceneActivation(SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate)
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
        GameObject GetLoadedSceneRoot(string rootName)
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
        void DumpLoadedSceneRoot()
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