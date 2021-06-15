/******************************************************************************/
/*!    \brief  シーンマネージャ.
*******************************************************************************/
using UnityEngine;
using System.Linq;
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
        const float LOAD_OPELATION_DELAY = 0.0f;
        const float SUBSCENE_ROOT_LOAD_DELAY = 0.3f;

        List<SceneSet>      loadedScenes = new List<SceneSet>();             // ロード済みのシーンルートと所属するサブシーンルート.
        Stack<string>       sceneHistory = new Stack<string>();              // シーンの遷移ヒストリ.
        bool                isFadeWaiting = false;
        CommonSceneUI       sceneUI = null;
        LoadingUiBase       currentLoadingUi = null;                         // 現在ロード中に表示しているUI
        bool                isDirectBoot;                                    // 直接起動かどうか
        bool                firstDirectBoot = true;                          // 直接起動の初回判定用
        public bool IsDirectBoot => isDirectBoot;
        SceneSet currentSceneRootSceneSet = null;

        // アンロードとロードタスク
        // loadOperationはloadingTaskとunloadingTask全てを解決するタスク
        // load/unloadが追加されるたびにタスクはloadOperationを待つ
        class LoadOperationSet
        {
            public string sceneName;
            public AsyncOperation sync;
            public bool isSubScene;
        }
        // 現在ロード目標としているシーン
        string currentAimLoadScene;
        string currentAimLoadSubScene;
        List<LoadOperationSet> loading = new List<LoadOperationSet>();
        List<LoadOperationSet> unloading = new List<LoadOperationSet>();
        Coroutine              loadOperation;
        Coroutine              sceneOperation;
        bool                   loadOperationRunning = false;

        [SceneName, SerializeField] string firstSceneName = default;
        [SceneName, SerializeField] string debugFirstSceneName = default;
        [SerializeField] bool isDebug = true;
        public static SceneManager Instance { get; set; }

        // 現在アクティブなシーンルート.
        public SceneRoot CurrentSceneRoot { get; private set; }
        public SubSceneRoot CurrentSubSceneRoot { get; private set; }
        public string CurrentSceneName { get { return CurrentSceneRoot.GetSceneName(); } }
        public string CurrentSubSceneName { get { return CurrentSubSceneRoot ? CurrentSubSceneRoot.GetSceneName() : null; } }
        public string CurrentPlayerSubSceneName { get; private set; }

        public bool IsLoadDone { get; private set; }                    // ロードUI含めロードが完了しているかどうか
        public bool IsLoadOperationRunning => loadOperationRunning;     // ロード処理が走っているかどうか

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
            // Startまできたら用済みなのでフラグ下げる
            isDirectBoot = false;
        }

        /// <summary>
        /// Awake this instance.
        /// </summary>
        void Awake()
        {
            // まずダミーシーンを非アクティブでロードしておく
            UnitySceneManager.LoadScene("dummy", UnityEngine.SceneManagement.LoadSceneMode.Additive);

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

        /// <summary>
        /// フェードしてロードUIの表示を待つまでのタスク
        /// </summary>
        IEnumerator WaitBeforeLoadForLoadingUi(SceneChangeFadeParam fadeParam)
        {
            isFadeWaiting = true;
            IsLoadDone = false;
            CmnFadeManager.Inst.StartFadeOut(EndFadeOutCallBack, fadeParam.fadeOutTime, fadeParam.fadeType, fadeParam.fadeColor);
            while (isFadeWaiting)
            {
                yield return null;
            }

            LoadingUIManager.Inst.ShowLoadingUI(fadeParam.loadingType, out currentLoadingUi);
            yield return currentLoadingUi.BeforeStartLoadProcess();
            //yield return null;  // 表示のために１フレ待つ.
        }
        /// <summary>
        /// ロード直後に、ロードUIが何かしらすることがあれば待つタスク
        /// </summary>
        IEnumerator WaitAfterLoadForLoadingUi(SceneChangeFadeParam fadeParam)
        {
            yield return currentLoadingUi.AfterStartLoadWaitProcess();
        }

        /// <summary>
        /// ロードすべきメインシーンを設定
        /// </summary>
        void SetLoadScene(string sceneName)
        {
            currentAimLoadScene = sceneName;
            currentAimLoadSubScene = null;
            RecreateLoadOperation(true, true);
        }

        /// <summary>
        /// ロードすべきサブシーンを設定
        /// </summary>
        public void ActiveAndApplySubScene(string subSceneName, bool updatePlayerSubscene)
        {
#if LOG_SCENE
            Debug.Log("ActiveAndApplySubScene:" + subSceneName);
#endif
            // 同じシーンが指定されたら早期リターン
            if(currentAimLoadSubScene == subSceneName)
            {
                return;
            }
            currentAimLoadSubScene = subSceneName;
            if(updatePlayerSubscene)
            {
                UpdatePlayerSubScne(subSceneName);
            }
            RecreateLoadOperation(false, false);
        }

        /// <summary>
        /// プレイヤーのいるサブシーンを更新
        /// </summary>
        public void UpdatePlayerSubScne(string subSceneName)
        {
            CurrentPlayerSubSceneName = subSceneName;
        }

        /// <summary>
        /// ロードオペレーション再生成
        /// </summary>
        void RecreateLoadOperation(bool loadImmidiate, bool unloadCurrent)
        {
            if (loadOperationRunning && loadOperation != null)
            {
                StopCoroutine(loadOperation);
#if LOG_SCENE
                Debug.Log("RecreateLoadOperation while");
#endif
            }
#if LOG_SCENE
            else
            {
                Debug.Log("RecreateLoadOperation");
            }
#endif
            loadOperation = StartCoroutine(LoadOperation(loadImmidiate, unloadCurrent));
        }

        /// <summary>
        /// ロードオペレーション
        /// </summary>
        IEnumerator LoadOperation(bool loadImmidiate, bool unloadCurrent)
        {
#if LOG_SCENE
            Debug.Log("loadope: start load operation");
#endif
            loadOperationRunning = true;
            // 既に走っているロード/アンロードがあれば先に待つ
            foreach (var item in unloading)
            {
                yield return SceneUnloadWait(item);
            }
#if LOG_SCENE
            Debug.Log("loadope: wait unload done");
#endif
            foreach (var item in loading)
            {
                if (item.sync != null)
                {
                    yield return SceneLoadWait(item);
                }
            }
#if LOG_SCENE
            Debug.Log("loadope: wait sceneloading done");
#endif

            // すべて片付いているのでいったんタスククリア
            loading.Clear();
            unloading.Clear();

            // 一定時間待つ
            if(!loadImmidiate)
            {
                yield return new WaitForSecondsRealtime(LOAD_OPELATION_DELAY);
            }
#if LOG_SCENE
            Debug.Log("loadope: wait operation delay done");
#endif

            // カレントアンロード→メインシーン→目標サブシーン→サブシーン必要シーンの順でロードを行う
            LoadOperationSet loadOperationSet;
            // カレントアンロード
            if(unloadCurrent && CurrentSceneRoot != null)
            {
#if LOG_SCENE
                Debug.Log("loadope: start current unload");
#endif
                // ロード済のサブシーンがあればサブシーンからアンロード
                if (CurrentSceneRoot.HasSubScene)
                {
                    foreach (var item in CurrentSceneRoot.SubSceneList)
                    {
                        if (GetLoadedSubSceneRoot(item) != null)
                        {
                            loadOperationSet = new LoadOperationSet();
                            loadOperationSet.sceneName = item;
                            loadOperationSet.isSubScene = true;
                            RemoveLoadedScene(loadOperationSet);
                            loadOperationSet.sync = UnitySceneManager.UnloadSceneAsync(item);
                            unloading.Add(loadOperationSet);
                        }
                    }
                }
                loadOperationSet = new LoadOperationSet();
                loadOperationSet.sceneName = CurrentSceneRoot.GetSceneName();
                RemoveLoadedScene(loadOperationSet);
                loadOperationSet.sync = UnitySceneManager.UnloadSceneAsync(CurrentSceneRoot.GetSceneName());
                SetCurrentSceneRoot(null);    // アンロード開始したらカレントメインシーンはなし
                unloading.Add(loadOperationSet);
                foreach (var item in unloading)
                {
                    if (item.sync != null)
                    {
                        yield return SceneUnloadWait(item);
                    }
                }
                unloading.Clear();
#if LOG_SCENE
                Debug.Log("loadope: current unload done");
#endif
            }
            // メインシーンロード
            // メインシーンがロードされていない場合はサブシーンのロード待ちを無視
            bool ignoreSubSceneLoadWait = false;
            if (GetLoadedSceneRoot(currentAimLoadScene) == null)
            {
                ignoreSubSceneLoadWait = true;
                loadOperationSet = new LoadOperationSet();
                loadOperationSet.sceneName = currentAimLoadScene;
#if LOG_SCENE
                Debug.Log("loadope: main load start : " + loadOperationSet.sceneName);
#endif
                yield return LoadInternal(loadOperationSet);
#if LOG_SCENE
                Debug.Log("loadope: main load done : " + loadOperationSet.sceneName);
#endif
            }
            SetCurrentSceneRoot(GetLoadedSceneRoot(currentAimLoadScene));

            // サブシーンが最初から設定されていない＝メインシーン初回サブシーンを指定
            var mainSceneRoot = GetLoadedSceneRoot(currentAimLoadScene);
            if (currentAimLoadSubScene == null && mainSceneRoot.HasSubScene)
            {
                currentAimLoadSubScene = mainSceneRoot.FirstSubSceneName;
            }

            // サブシーン指定があれば指定サブシーン、なければメインシーンの初回シーン
            var loadReserve = new List<LoadOperationSet>();
            if(currentAimLoadSubScene != null)
            {
                if (GetLoadedSubSceneRoot(currentAimLoadSubScene) == null)
                {
#if LOG_SCENE
                    Debug.Log("loadope: start aim load : " + currentAimLoadSubScene);
#endif
                    loadOperationSet = new LoadOperationSet();
                    loadOperationSet.sceneName = currentAimLoadSubScene;
                    loadOperationSet.isSubScene = true;
                    yield return LoadInternal(loadOperationSet);
#if LOG_SCENE
                    Debug.Log("loadope: aim load done : " + currentAimLoadSubScene);
#endif
                }
                // 目標サブシーンまで読み込んだ時点で、
                // 必要サブシーンをロードリストに追加し、既にロードされている不要サブシーンをすべてアンロード
                if (CurrentSubSceneRoot != null && CurrentSubSceneRoot.DirectionalLight != null)    // 直前のディレクショナルライト消す
                {
                    CurrentSubSceneRoot.DirectionalLight.gameObject.SetActive(false);
                }
                CurrentSubSceneRoot = GetLoadedSubSceneRoot(currentAimLoadSubScene);
                foreach (var item in CurrentSubSceneRoot.RequireSubSceneNames)
                {
                    // 既にロード済でないもの
                    if (GetLoadedSubSceneRoot(item) == null)
                    {
                        var newOpe = new LoadOperationSet();
                        newOpe.sceneName = item;
                        newOpe.isSubScene = true;
                        loadReserve.Add(newOpe);
                    }
                }
                foreach (var item in loadedScenes)
                {
                    foreach (var sub in item.subSceneRoots)
                    {
                        // 必要シーンにふくまれていなくて、メインサブシーンでもないもの
                        var subSceneName = sub.GetSceneName();
                        if (subSceneName != currentAimLoadSubScene &&
                            !CurrentSubSceneRoot.RequireSubSceneNames.Contains(subSceneName))
                        {
                            var newOpe = new LoadOperationSet();
                            newOpe.sceneName = subSceneName;
                            newOpe.isSubScene = true;
                            unloading.Add(newOpe);
                        }
                    }
                }
                // 必要サブシーンのロードはアンロード後
                // 一気にアンロードしてから１シーンずつロード
                foreach (var item in unloading)
                {
#if LOG_SCENE
                    Debug.Log("loadope: start unload subscene : " + item.sceneName);
#endif
                    RemoveLoadedScene(item);
                    item.sync = UnitySceneManager.UnloadSceneAsync(item.sceneName);
                }
                foreach (var item in unloading)
                {
                    yield return SceneUnloadWait(item);
#if LOG_SCENE
                    Debug.Log("loadope: Unload subscene done : " + item.sceneName);
#endif
                }

                // MEMO:負荷分散のために、ロードは最後行う
            }

            // サブシーンを持っていなければシーンルートをアクティブにする.
            if (!CurrentSceneRoot.HasSubScene)
            {
                if (UnitySceneManager.GetActiveScene().name != currentAimLoadScene && UnitySceneManager.SetActiveScene(UnitySceneManager.GetSceneByName(currentAimLoadScene)) == false)
                {
                    Debug.LogError("active scene fail:" + currentAimLoadScene);
                }
            }
            else
            {
                if (UnitySceneManager.GetActiveScene().name != currentAimLoadSubScene && UnitySceneManager.SetActiveScene(UnitySceneManager.GetSceneByName(currentAimLoadSubScene)) == false)
                {
                    Debug.LogError("active scene fail:" + currentAimLoadSubScene);
                }
                // 必要サブシーンが読まれたらプレイヤーサブシーンを更新
                UpdatePlayerSubScne(CurrentSubSceneRoot.GetSceneName());
            }
#if LOG_SCENE
            Debug.Log("end active scene");
#endif

            // サブシーンのディレクショナルライト設定
            if (CurrentSubSceneRoot && CurrentSubSceneRoot.DirectionalLight != null)
            {
                CurrentSubSceneRoot.DirectionalLight.gameObject.SetActive(true);
            }

            // ライトプローブ再計算
            LightProbes.TetrahedralizeAsync();

            // ディレイを入れてから、最後に必要サブシーンをロードする
            if(currentAimLoadSubScene != null)
            {
                // ゲーム再開時にサブシーントリガーの上にいた場合に念のため配慮
                if(!ignoreSubSceneLoadWait)
                {
                    // サブシーンのロード処理は、サブシーンアクティブトリガーのやり取りをしているときは負荷分散のために止める
                    while (SubSceneActivateTrigger.IsSubsceneTriggerWorking)
                    {
                        yield return null;
                    }
                    yield return new WaitForSecondsRealtime(SUBSCENE_ROOT_LOAD_DELAY);
                }
                loading.AddRange(loadReserve);
                foreach (var item in loading)
                {
#if LOG_SCENE
                    Debug.Log("loadope: start load subscene : " + item.sceneName);
#endif
                    yield return LoadInternal(item);
#if LOG_SCENE
                    Debug.Log("loadope: load subscene done : " + item.sceneName);
#endif
                }
            }

            loadOperationRunning = false;
#if LOG_SCENE
            Debug.Log("loadope: end load operation");
#endif
        }
        IEnumerator LoadInternal(LoadOperationSet set)
        {
#if LOG_SCENE
            Debug.Log("start load :" + set.sceneName);
#endif
            // ロード済なら何もしない
            if ((set.isSubScene && GetLoadedSceneRoot(set.sceneName) == null)
                ||
                (!set.isSubScene && loadedScenes.Find(s => s.sceneRoot.GetSceneName() == set.sceneName) == null))
            {
                set.sync = UnitySceneManager.LoadSceneAsync(set.sceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
                set.sync.allowSceneActivation = true;
                yield return SceneLoadWait(set);
            }
#if LOG_SCENE
            Debug.Log("end load :" + set.sceneName);
#endif
        }
        IEnumerator SceneLoadWait(LoadOperationSet set)
        {
            while (set.sync.isDone == false || set.sync.progress < 1.0f)
            {
                yield return null;
            }
            // ロード済セット追加待ち
            while ((!set.isSubScene && GetLoadedSceneRoot(set.sceneName) == null)
                 ||
                 (set.isSubScene && GetLoadedSubSceneRoot(set.sceneName) == null))
            {
                yield return null;
            }
        }
        IEnumerator SceneUnloadWait(LoadOperationSet set)
        {
            while (set.sync.isDone == false)
            {
                yield return null;
            }
        }

        void RemoveLoadedScene(LoadOperationSet set)
        {
            // ロード済セットから削除
            if (!set.isSubScene)
            {
                var del = loadedScenes.Find(s => s.sceneRoot.GetSceneName() == set.sceneName);
                if (del != null)
                {
                    loadedScenes.Remove(del);
                }
            }
            else
            {
                foreach (var scene in loadedScenes)
                {
                    if (set.isSubScene)
                    {
                        var del = scene.subSceneRoots.Find(s => s.GetSceneName() == set.sceneName);
                        if (del != null)
                        {
                            scene.subSceneRoots.Remove(del);
                        }
                    }
                }
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
        /// <param name="nextSceneName">シーン名.</param>
        /// <param name="fadeTime">フェードパラメータ.</param>
        /// <param name="pushAnchor">シーンまでのアンカー.</param>
        public void PushScene(string nextSceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate = null)
        {
            if (sceneOperation != null) StopCoroutine(sceneOperation);
            sceneOperation = StartCoroutine(PushSceneInternal(nextSceneName, fadeParam, afterSceneControlDelegate));
        }
        IEnumerator PushSceneInternal(string nextSceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate)
        {
            yield return WaitBeforeLoadForLoadingUi(fadeParam);

            sceneHistory.Push(nextSceneName);
            SetLoadScene(nextSceneName);

            yield return WaitAfterLoadForLoadingUi(fadeParam);

            while (loadOperationRunning)
            {
                yield return null;
            }
            yield return WaitEndLoadUi();
            CleaneUpAfterChangeSceneActivation(fadeParam, afterSceneControlDelegate);
        }

        /// <summary>
        /// シーン変更.
        /// </summary>
        /// <param name="nextSceneName">シーン名.</param>
        /// <param name="fadeTime">フェードパラメータ.</param>
        public void ChangeScene(string nextSceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate = null)
        {
            if (sceneOperation != null) StopCoroutine(sceneOperation);
            sceneOperation = StartCoroutine(ChangeSceneInternal(nextSceneName, fadeParam, afterSceneControlDelegate));
        }
        IEnumerator ChangeSceneInternal(string nextSceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate)
        {
            yield return WaitBeforeLoadForLoadingUi(fadeParam);

            if (sceneHistory.Count > 0)
            {
                sceneHistory.Pop();
                sceneHistory.Push(nextSceneName);
                SetLoadScene(nextSceneName);
                yield return WaitAfterLoadForLoadingUi(fadeParam);
                while (loadOperationRunning)
                {
                    yield return null;
                }
            }
            else
            {
                yield return WaitAfterLoadForLoadingUi(fadeParam);
            }
            yield return WaitEndLoadUi();
            CleaneUpAfterChangeSceneActivation(fadeParam, afterSceneControlDelegate);
        }

        /// <summary>
        /// シーンポップ.
        /// </summary>
        /// <param name="fadeTime">フェードパラメータ.</param>
        public void PopScene(SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate = null)
        {
            if (sceneOperation != null) StopCoroutine(sceneOperation);
            sceneOperation = StartCoroutine(PopSceneInternal(fadeParam, afterSceneControlDelegate));
        }
        IEnumerator PopSceneInternal(SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate)
        {
            yield return WaitBeforeLoadForLoadingUi(fadeParam);
            if (sceneHistory.Count > 0)
            {
                string nextSceneName = sceneHistory.Peek();
                sceneHistory.Pop();
                SetLoadScene(nextSceneName);
                yield return WaitAfterLoadForLoadingUi(fadeParam);
                while (loadOperationRunning)
                {
                    yield return null;
                }
            }
            else
            {
                yield return WaitAfterLoadForLoadingUi(fadeParam);
            }
            yield return WaitEndLoadUi();
            CleaneUpAfterChangeSceneActivation(fadeParam, afterSceneControlDelegate);
        }

        /// <summary>
        /// 指定シーンまでシーンポップ.
        /// </summary>
        /// <param name="nextSceneName">ポップ先のシーン名.</param>
        /// <param name="fadeTime">フェードパラメータ.</param>
        public void PopSceneTo(string nextSceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate = null)
        {
            // アンカーが無ければ警告出して無視
            bool bFound = false;
            foreach (var item in sceneHistory)
            {
                if (item == nextSceneName)
                {
                    if (sceneOperation != null) StopCoroutine(sceneOperation);
                    sceneOperation = StartCoroutine(PopSceneToInternal(nextSceneName, fadeParam, afterSceneControlDelegate));
                    bFound = true;
                    break;
                }
            }
            if(bFound == false)
            {
                Debug.LogWarning("Anchor not found.:" + nextSceneName);
            }
        }
        IEnumerator PopSceneToInternal(string nextSceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate)
        {
            yield return WaitBeforeLoadForLoadingUi(fadeParam);

            // 最初に見つかった同名シーンまでポップ.
            Stack<string> sceneHistoryCopy = new Stack<string>(sceneHistory.Reverse());
            int popCount = 0;
            while (sceneHistoryCopy.Count > 0)
            {
                if(nextSceneName != sceneHistoryCopy.Peek())
                {
                    ++popCount;
                    sceneHistoryCopy.Pop();
                }
                else
                {
                    break;
                }
            }
            if (popCount >= sceneHistory.Count)
            {
                Debug.LogError("invalid scene name:" + nextSceneName);
            }
            else
            {
                for (int i = 0; i < popCount; i++)
                {
                    sceneHistory.Pop();
                }
                string peekedNextSceneName = sceneHistory.Peek();
                SetLoadScene(peekedNextSceneName);
                yield return WaitAfterLoadForLoadingUi(fadeParam);
                while (loadOperationRunning)
                {
                    yield return null;
                }
            }
            yield return WaitEndLoadUi();
            CleaneUpAfterChangeSceneActivation(fadeParam, afterSceneControlDelegate);
        }

        ///// <summary>
        ///// 現在読まれているシーン以外をすべてアンロード.
        ///// </summary>
        //public void UnloadAllOtherScene()
        //{
        //    SceneSet lastSceneSet = null;
        //    foreach (var scene in loadedScenes) 
        //    {
        //        if(scene.sceneRoot != CurrentSceneRoot)
        //        {
        //            foreach (var item in scene.subSceneRoots)
        //            {
        //                UnitySceneManager.UnloadSceneAsync(item.GetSceneName());
        //            }
        //            UnitySceneManager.UnloadSceneAsync(scene.sceneRoot.GetSceneName());
        //        }
        //        else
        //        {
        //            lastSceneSet = scene;
        //        }
        //    }

        //    loadedScenes = new List<SceneSet>();
        //    loadedScenes.Add (lastSceneSet);
        //}

        ///// <summary>
        ///// シーンをすべてアンロード.
        ///// </summary>
        //public void DebugUnloadAllScene()
        //{
        //    // Sceneがすでにロードされているなら、そのシーンをアクティブにする.
        //    foreach (var scene in loadedScenes) 
        //    {
        //        foreach (var item in scene.subSceneRoots)
        //        {
        //            UnitySceneManager.UnloadSceneAsync(scene.sceneRoot.GetSceneName());
        //        }
        //        UnitySceneManager.UnloadSceneAsync(scene.sceneRoot.GetSceneName());
        //    }
        //    loadedScenes = new List<SceneSet>();
        //}

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
            if (CurrentSceneRoot)
            {
                CurrentSceneRoot.BroadcastMessage(CmnMonoBehaviour.FADE_END_NAME, SendMessageOptions.DontRequireReceiver);
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
            if(CurrentSceneRoot == null && isDirectBoot && firstDirectBoot)
            {
                firstDirectBoot = false;
                SetCurrentSceneRoot(sceneRoot);
                currentAimLoadScene = sceneRoot.GetSceneName();
                sceneHistory.Push(CurrentSceneRoot.GetSceneName());
                if(CurrentSceneRoot.HasSubScene)
                {
                    ActiveAndApplySubScene(CurrentSceneRoot.FirstSubSceneName, true);
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
                Debug.LogWarning("設定が不正です。読み込まれた一番上を親シーンかつActiveにし、親シーンの設定を確認してください。サブシーン単体起動なら無視してください。:" + subSceneRoot.name + " 親:" + subSceneRoot.ParentSceneName);
            }
        }

        /// <summary>
        /// シーン切り替え後のクリーンアップ（フェードイン等）.
        /// </summary>
        void CleaneUpAfterChangeSceneActivation(SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate)
        {
            LoadingUIManager.Inst.HideLoadingUI(fadeParam.loadingType);
            CmnFadeManager.Inst.StartFadeIn(EndFadeInCallBack, fadeParam.fadeInTime, fadeParam.fadeType, fadeParam.fadeColor);
            sceneUI.ChangeCommonSceneUI(CurrentSceneRoot.SceneUiParam, CurrentSceneRoot.SceneBgKind);
            sceneUI.ChangeSceneTitleLabel(CurrentSceneRoot.SceneNameLocalizeID);
            afterSceneControlDelegate?.Invoke();
        }

        /// <summary>
        /// シーンルート更新
        /// </summary>
        void SetCurrentSceneRoot(SceneRoot set)
        {
            CurrentSceneRoot = set;
            currentSceneRootSceneSet = null;
            if (CurrentSceneRoot != null)
            {
                foreach (var scene in loadedScenes)
                {
                    if (scene.sceneRoot == CurrentSceneRoot)
                    {
                        currentSceneRootSceneSet = scene;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// ロードが終了しているシーンルートを取得する.
        /// </summary>
        SceneRoot GetLoadedSceneRoot(string sceneName)
        {
            foreach (var scene in loadedScenes) 
            {
                if(scene.sceneRoot.GetSceneName() == sceneName)
                {
                    return scene.sceneRoot;
                }
            }
            return null;
        }

        /// <summary>
        /// ロードが終了しているサブシーンルートを取得する.
        /// </summary>
        SubSceneRoot GetLoadedSubSceneRoot(string subSceneName)
        {
            foreach (var scene in loadedScenes) 
            {
                foreach (var item in scene.subSceneRoots)
                {
                    if (item.GetSceneName() == subSceneName)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// ロードが終了しているサブシーンルートを取得する.
        /// </summary>
        public IReadOnlyList<SubSceneRoot> GetCurrentSubSceneRoots()
        {
            if(currentSceneRootSceneSet != null)
            {
                return currentSceneRootSceneSet.subSceneRoots;
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

        /// <summary>
        /// ロードUIの終了を待機
        /// </summary>
        IEnumerator WaitEndLoadUi()
        {
            //// サブシーンのロード開始が１フレまっているので、サブシーンのロードがないか確認するために1フレ待つ
            //yield return null;

            while(loadOperationRunning)
            {
                yield return null;
            }

            IsLoadDone = true;

            if (currentLoadingUi != null)
            {
                while (!currentLoadingUi.IsEnd)
                {
                    yield return null;
                }
            }
            currentLoadingUi = null;
        }
    }
}