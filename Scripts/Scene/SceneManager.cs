/******************************************************************************/
/*!    \brief  シーンマネージャ.
*******************************************************************************/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
namespace VMUnityLib
{
    public delegate void AfterSceneControlDelegate();

    /// <summary>
    /// 子シーンは一度ロードされるとゲーム終了時までオンメモリ。以降は必要なときだけアクティブにする.
    /// </summary>
    public class SceneManager : MonoBehaviour
    {
        public const string SCENE_ROOT_NAME_HEADER = "SceneRoot_";
        public const string SUBSCENE_ROOT_NAME_HEADER = "SubSceneRoot_";
        const float DEBGUG_DUMMY_LOAD_TIME = 0.5f;
        const float DUMMY_LOAD_TIME = 2.0f;

        SceneRoot           currentSceneRoot = null;                         // 現在アクティブなシーンルート.
        List<SceneSet>      loadedScenes = new List<SceneSet>();             // ロード済みのシーンルートと所属するサブシーンルート.
        Stack<string>       sceneHistory = new Stack<string>();              // シーンの遷移ヒストリ.
        bool                isFadeWaiting = false;
        CommonSceneUI       sceneUI = null;
        bool                isDirectBoot;                                   // 直接起動かどうか

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
        /// シーンルートと所属するサブシーンルート
        /// </summary>
        public class SceneSet
        {
            public SceneRoot            sceneRoot;
            public List<SubSceneRoot>   subSceneRoots;
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
            SceneSet lastSceneSet = null;
            foreach (var scene in loadedScenes) 
            {
                if(scene.sceneRoot != currentSceneRoot)
                {
                    foreach (var item in scene.subSceneRoots)
                    {
                        UnitySceneManager.UnloadSceneAsync(item.GetSceneName());
                    }
                    UnitySceneManager.UnloadSceneAsync(scene.sceneRoot.GetSceneName());
                }
                else
                {
                    lastSceneSet = scene;
                }
            }

            loadedScenes = new List<SceneSet>();
            loadedScenes.Add (lastSceneSet);
        }

        /// <summary>
        /// シーンをすべてアンロード.
        /// </summary>
        public void DebugUnloadAllScene()
        {
            // Sceneがすでにロードされているなら、そのシーンをアクティブにする.
            foreach (var scene in loadedScenes) 
            {
                foreach (var item in scene.subSceneRoots)
                {
                    UnitySceneManager.UnloadSceneAsync(scene.sceneRoot.GetSceneName());
                }
                UnitySceneManager.UnloadSceneAsync(scene.sceneRoot.GetSceneName());
            }
            loadedScenes = new List<SceneSet>();
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
        IEnumerator ChangeSceneActivation(string sceneName)
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
            foreach (var scene in loadedScenes) 
                {
                if(scene.sceneRoot.GetSceneName() == sceneName)
                {
                    scene.sceneRoot.SetSceneActive();
                    currentSceneRoot = scene.sceneRoot;
                    isLoaded = true;
                    break;
                }
            }

            //ロードされていなかったらAddiveLoadする.
            SceneRoot sceneRoot = null;
            if (isLoaded == false)
            {
                yield return LoadSceneInternal(sceneName, false);

                sceneRoot = GetLoadedSceneRoot(sceneName).GetComponent<SceneRoot>();

                // UNITYのバージョンアップにともない、マルチシーン機能が実装されたため親子関係による変更は廃止
                //sceneRootObj.transform.parent = gameObject.transform;
                currentSceneRoot = sceneRoot;
                currentSceneRoot.SetSceneActive();

                // 初回に必要なサブシーンをロードする
                if (sceneRoot.HasSubScene)
                {
                    yield return LoadFirstSubScene(sceneRoot);
                }
            }

            // サブシーンを持っていなければシーンルートをアクティブにする.
            if (!sceneRoot) sceneRoot = GetLoadedSceneRoot(sceneName).GetComponent<SceneRoot>();
            if (!sceneRoot.HasSubScene && UnitySceneManager.SetActiveScene(UnitySceneManager.GetSceneByName(sceneName)) == false)
            {
                Debug.Log("active scene fail");
            }
        }

        /// <summary>
        /// 初回に必要なサブシーンロード
        /// </summary>
        IEnumerator LoadFirstSubScene(SceneRoot sceneRoot)
        {
            // 重複ロード防止のため1フレ待つ。既にロードされてたらやめとく
            yield return null;
            if(!IsLoadedSubScene(sceneRoot.FirstSubSceneName))
            {
                yield return LoadSceneInternal(sceneRoot.FirstSubSceneName, true);
            }

            // 初回サブシーンが読み終わったら、必要サブシーンを読み込む
            SubSceneRoot subSceneRoot = GetLoadedSubSceneRoot(sceneRoot.FirstSubSceneName).GetComponent<SubSceneRoot>();
            foreach (var item in subSceneRoot.RequireSubSceneNames)
            {
                if (!IsLoadedSubScene(item))
                {
                    yield return LoadSceneInternal(item, true);
                }
            }

            // サブシーンを持っている場合は、初回ロードのサブシーンをアクティブにする
            if (sceneRoot.HasSubScene && UnitySceneManager.SetActiveScene(UnitySceneManager.GetSceneByName(sceneRoot.FirstSubSceneName)) == false)
            {
                Debug.Log("active scene fail");
            }
        }

        /// <summary>
        /// サブシーンが読み込まれていたかどうか
        /// </summary>
        public bool IsLoadedSubScene(string subSceneName)
        {
            bool alreadyLoaded = false;
            foreach (var item in loadedScenes)
            {
                if (item.subSceneRoots.Exists(s => subSceneName == s.GetSceneName()))
                {
                    alreadyLoaded = true;
                    break;
                }
            }
            return alreadyLoaded;
        }

        /// <summary>
        /// シーンロード内部処理
        /// </summary>
        IEnumerator LoadSceneInternal(string sceneName, bool isSubScene)
        {
            AsyncOperation async = UnitySceneManager.LoadSceneAsync(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            async.allowSceneActivation = true;
            while (async.isDone == false || async.progress < 1.0f)
            {
                yield return LibBridgeInfo.WaitForEndOfFrame;
            }

            Func<string, GameObject> getLoadedSceneRoot = GetLoadedSceneRoot;
            if (isSubScene) getLoadedSceneRoot = GetLoadedSubSceneRoot;

            // 120フレーム待ってもアクティブなルートが取れない場合は原因不明のエラーとしてassert
            GameObject sceneRootObj = getLoadedSceneRoot(sceneName);
            int waitCnt = 0;
            while (sceneRootObj == null && waitCnt < 120)
            {
                ++waitCnt;
                sceneRootObj = getLoadedSceneRoot(sceneName);
                yield return LibBridgeInfo.WaitForEndOfFrame;
            }
            if (sceneRootObj == null)
            {
                Debug.LogAssertion("Scene root or sub scene not found:" + sceneName + " waited" + waitCnt + "frame.");
                DumpLoadedSceneRoot();

                UnitySceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
                sceneRootObj = getLoadedSceneRoot(sceneName);
                if (sceneRootObj == null)
                {
                    Debug.LogAssertion("no sync load but Scene root or sub scene not found:" + sceneName);
                    DumpLoadedSceneRoot();
                }
            }
        }

        /// <summary>
        /// 指定サブシーンに必要なサブシーンのみをロードし、そのほかのシーンをアンロードする
        /// </summary>
        public void ActiveAndApplySubScene(SubSceneRoot subSceneRoot)
        {
            if(subSceneRoot.GetSceneName() != UnitySceneManager.GetActiveScene().name)
            {
                StopCoroutine("ActiveAndApplySubSceneInternal");
                StartCoroutine(ActiveAndApplySubSceneInternal(subSceneRoot));
            }
        }
        IEnumerator ActiveAndApplySubSceneInternal(SubSceneRoot subSceneRoot)
        {
            // ロード済のシーンから自分の親シーンを探す
            var subSceneRootName = subSceneRoot.GetSceneName();
            var unloadSceneList = new List<string>();
            var loadSceneList = new List<string>();
            foreach (var scene in loadedScenes)
            {
                if(scene.sceneRoot.GetSceneName() == subSceneRoot.ParentSceneName)
                {
                    // 既にロード済のサブシーンが新たなアクティブサブシーンの必要シーンに含まれていなければアンロード
                    foreach (var item in scene.subSceneRoots)
                    {
                        var loadedSubSceneName = item.GetSceneName();
                        if(subSceneRootName != loadedSubSceneName && !subSceneRoot.RequireSubSceneNames.Contains(loadedSubSceneName))
                        {
                            unloadSceneList.Add(loadedSubSceneName);
                        }
                    }

                    // 新たなアクティブサブシーンの必要シーンがロードされていなければロード
                    // HACK:サブシーンはゲームオブジェクトのオンオフではなく完全にロードアンロードのみの仕様とする
                    foreach (var item in subSceneRoot.RequireSubSceneNames)
                    {
                        if(!scene.subSceneRoots.Exists(s => item == s.GetSceneName()))
                        {
                            loadSceneList.Add(item);
                        }
                    }
                    break;
                }
            }

            // 参照する前にロード済リストから削除
            foreach (var item in unloadSceneList)
            {
                foreach (var scene in loadedScenes)
                {
                    scene.subSceneRoots.RemoveAll(sub => sub.GetSceneName() == item);
                }
            }
            foreach (var item in unloadSceneList)
            {
                yield return UnitySceneManager.UnloadSceneAsync(item);
            }
            foreach (var item in loadSceneList)
            {
                yield return LoadSceneInternal(item, true);
            }

            if (UnitySceneManager.SetActiveScene(UnitySceneManager.GetSceneByName(subSceneRootName)) == false)
            {
                Debug.Log("active scene fail");
            }
        }

        /// <summary>
        /// ロード済シーンルートを追加する（SceneRootが自己申告）.
        /// </summary>
        public void AddLoadedSceneRoot(SceneRoot sceneRoot)
        {
            bool containScene = false;
            foreach (var scene in loadedScenes)
            {
                if(scene.sceneRoot == sceneRoot)
                {
                    containScene = true;
                    break;
                }
            }
            if (!containScene)
            {
                var newSceneSet = new SceneSet();
                newSceneSet.sceneRoot = sceneRoot;
                newSceneSet.subSceneRoots = new List<SubSceneRoot>();
                loadedScenes.Add(newSceneSet);
            }

            // 直接起動の場合は現在のシーン更新してシーンヒストリーに入れる
            if(currentSceneRoot == null && isDirectBoot)
            {
                currentSceneRoot = sceneRoot;
                sceneHistory.Push(currentSceneRoot.GetSceneName());
                // どうせエディタ専用機能なのでコルーチンでサブシーンロード
                if (sceneRoot.HasSubScene)
                {
                    StartCoroutine(LoadFirstSubScene(sceneRoot));
                }
            }
        }

        /// <summary>
        /// ロード済サブシーンルートを追加する（SubSceneRootが自己申告）.
        /// </summary>
        public void AddLoadedSubSceneRoot(SubSceneRoot subSceneRoot)
        {
            // ロード済みの所属サブシーンに追加する
            // 親シーンが読み込まれている前提
            bool foundParentScene = false;
            foreach (var scene in loadedScenes)
            {
                if (scene.sceneRoot.GetSceneName() == subSceneRoot.ParentSceneName)
                {
                    foundParentScene = true;
                    scene.subSceneRoots.Add(subSceneRoot);
                    break;
                }
            }
            if(!foundParentScene)
            {
                Debug.LogError("親シーンが読み込まれていないか、親シーン設定がおかしいです:" + subSceneRoot.name + " 親:" + subSceneRoot.ParentSceneName);
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
            afterSceneControlDelegate?.Invoke();
        }

        /// <summary>
        /// ロードが終了しているシーンルートを取得する.
        /// </summary>
        GameObject GetLoadedSceneRoot(string sceneName)
        {
            foreach (var scene in loadedScenes) 
            {
                if(scene.sceneRoot.GetSceneName() == sceneName)
                {
                    return scene.sceneRoot.gameObject;
                }
            }
            return null;
        }

        /// <summary>
        /// ロードが終了しているサブシーンルートを取得する.
        /// </summary>
        GameObject GetLoadedSubSceneRoot(string subSceneName)
        {
            foreach (var scene in loadedScenes) 
            {
                foreach (var item in scene.subSceneRoots)
                {
                    if (item.GetSceneName() == subSceneName)
                    {
                        return item.gameObject;
                    }
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
            foreach (var scene in loadedScenes) 
            {
                Debug.Log(" " + scene.sceneRoot.gameObject.name);
                foreach (var subRoot in scene.subSceneRoots)
                {
                    Debug.Log(" " + subRoot.gameObject.name);
                }
            }
            Debug.Log("-------");
        }
    }
}