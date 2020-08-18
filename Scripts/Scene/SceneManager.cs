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

        List<SceneSet>      loadedScenes = new List<SceneSet>();             // ロード済みのシーンルートと所属するサブシーンルート.
        Stack<string>       sceneHistory = new Stack<string>();              // シーンの遷移ヒストリ.
        bool                isFadeWaiting = false;
        CommonSceneUI       sceneUI = null;
        LoadingUiBase       currentLoadingUi = null;                         // 現在ロード中に表示しているUI
        bool                isDirectBoot;                                    // 直接起動かどうか
        bool                firstDirectBoot = true;                          // 直接起動の初回判定用
        List<string>        loadingSubScene = new List<string>();

        Coroutine LoadSubSceneInternalCoroutine = null;
        Coroutine ActiveAndApplySubSceneInternalCoroutine = null;

        [SceneName, SerializeField] string firstSceneName = default;
        [SceneName, SerializeField] string debugFirstSceneName = default;
        [SerializeField] bool isDebug = true;
        public static SceneManager Instance { get; set; }

        // 現在アクティブなシーンルート.
        public SceneRoot CurrentSceneRoot { get; private set; }
        public SubSceneRoot CurrentSubSceneRoot { get; private set; }
        public string CurrentSceneName { get { return CurrentSceneRoot.GetSceneName(); } }
        public string CurrentSubSceneName { get { return CurrentSubSceneRoot ? CurrentSubSceneRoot.GetSceneName() : null; } }

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
        /// <param name="nextSceneName">シーン名.</param>
        /// <param name="fadeTime">フェードパラメータ.</param>
        /// <param name="pushAnchor">シーンまでのアンカー.</param>
        public void PushScene(string nextSceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate = null, bool unloadOtherScene = true)
        {
            StopAllCoroutines();
            isFadeWaiting = true;
            CmnFadeManager.Inst.StartFadeOut(EndFadeOutCallBack, fadeParam.fadeOutTime, fadeParam.fadeType, fadeParam.fadeColor);
            StartCoroutine(PushSceneInternal(nextSceneName, fadeParam, afterSceneControlDelegate, unloadOtherScene));
        }
        IEnumerator PushSceneInternal(string nextSceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate, bool unloadOtherScene)
        {
            while (isFadeWaiting)
            {
                yield return null;
            }

            LoadingUIManager.Inst.ShowLoadingUI(fadeParam.loadingType,out currentLoadingUi);
            yield return null;  // 表示のために１フレ待つ.

            // 他シーンアンロードフラグがたってたら一つだけシーンロード
            if (unloadOtherScene)
            {
                yield return LoadOneSceneInternal(nextSceneName);
            }
            else
            {
                yield return StartCoroutine(ChangeSceneActivation(nextSceneName, unloadOtherScene));
            }
            sceneHistory.Push(nextSceneName);

            // 表示中のロードUIの処理が終わるまで待機
            yield return WaitEndLoadUi();

            CleaneUpAfterChangeSceneActivation(fadeParam, afterSceneControlDelegate);
        }

        /// <summary>
        /// シーン変更.
        /// </summary>
        /// <param name="nextSceneName">シーン名.</param>
        /// <param name="fadeTime">フェードパラメータ.</param>
        public void ChangeScene(string nextSceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate = null, bool unloadOtherScene = true)
        {
            StopAllCoroutines();
            isFadeWaiting = true;
            CmnFadeManager.Inst.StartFadeOut(EndFadeOutCallBack, fadeParam.fadeOutTime, fadeParam.fadeType, fadeParam.fadeColor);
            StartCoroutine(ChangeSceneInternal(nextSceneName, fadeParam, afterSceneControlDelegate, unloadOtherScene));
        }
        IEnumerator ChangeSceneInternal(string nextSceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate, bool unloadOtherScene)
        {
            while (isFadeWaiting)
            {
                yield return null;
            }

            LoadingUIManager.Inst.ShowLoadingUI(fadeParam.loadingType, out currentLoadingUi);
            yield return null;  // 表示のために１フレ待つ.

            if (sceneHistory.Count > 0)
            {
                sceneHistory.Pop();
                sceneHistory.Push(nextSceneName);

                // 他シーンアンロードフラグがたってるか、同じシーンの指定なら一つだけシーンロード
                if (nextSceneName == CurrentSceneName || (nextSceneName != CurrentSceneName && unloadOtherScene))
                {
                    yield return LoadOneSceneInternal(nextSceneName);
                }
                else
                {
                    yield return ChangeSceneActivation(nextSceneName, unloadOtherScene);
                }
            }

            // 表示中のロードUIの処理が終わるまで待機
            yield return WaitEndLoadUi();

            CleaneUpAfterChangeSceneActivation(fadeParam, afterSceneControlDelegate);
        }

        /// <summary>
        /// シーンポップ.
        /// </summary>
        /// <param name="fadeTime">フェードパラメータ.</param>
        public void PopScene(SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate = null, bool unloadOtherScene = true)
        {
            StopAllCoroutines();
            isFadeWaiting = true;
            CmnFadeManager.Inst.StartFadeOut(EndFadeOutCallBack, fadeParam.fadeOutTime, fadeParam.fadeType, fadeParam.fadeColor);
            StartCoroutine(PopSceneInternal(fadeParam, afterSceneControlDelegate, unloadOtherScene));
        }
        IEnumerator PopSceneInternal(SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate, bool unloadOtherScene)
        {
            while (isFadeWaiting)
            {
                yield return null;
            }

            LoadingUIManager.Inst.ShowLoadingUI(fadeParam.loadingType, out currentLoadingUi);
            yield return null;  // 表示のために１フレ待つ.

            if (sceneHistory.Count > 0)
            {
                sceneHistory.Pop();
                string nextSceneName = sceneHistory.Peek();

                // 他シーンアンロードフラグがたってたら一つだけシーンロード
                if (unloadOtherScene)
                {
                    yield return LoadOneSceneInternal(nextSceneName);
                }
                else
                {
                    yield return ChangeSceneActivation(nextSceneName, unloadOtherScene);
                }
            }

            // 表示中のロードUIの処理が終わるまで待機
            yield return WaitEndLoadUi();

            CleaneUpAfterChangeSceneActivation(fadeParam, afterSceneControlDelegate);
        }

        /// <summary>
        /// 指定シーンまでシーンポップ.
        /// </summary>
        /// <param name="nextSceneName">ポップ先のシーン名.</param>
        /// <param name="fadeTime">フェードパラメータ.</param>
        public void PopSceneTo(string nextSceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate = null, bool unloadOtherScene = true)
        {
            StopAllCoroutines();
            isFadeWaiting = true;

            // アンカーが無ければ警告出して無視
            bool bFound = false;
            foreach (var item in sceneHistory)
            {
                if (item == nextSceneName)
                {
                    CmnFadeManager.Inst.StartFadeOut(EndFadeOutCallBack, fadeParam.fadeOutTime, fadeParam.fadeType, fadeParam.fadeColor);
                    StartCoroutine(PopSceneToInternal(nextSceneName, fadeParam, afterSceneControlDelegate, unloadOtherScene));
                    bFound = true;
                    break;
                }
            }
            if(bFound == false)
            {
                Debug.LogWarning("Anchor not found.:" + nextSceneName);
            }
        }
        IEnumerator PopSceneToInternal(string nextSceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate, bool unloadOtherScene)
        {
            while (isFadeWaiting)
            {
                yield return null;
            }

            LoadingUIManager.Inst.ShowLoadingUI(fadeParam.loadingType, out currentLoadingUi);
            yield return null;  // 表示のために１フレ待つ.

            // 最初に見つかった同名シーンまでポップ.
            Stack<string> sceneHistoryCopy = new Stack<string>(sceneHistory);
            sceneHistoryCopy.Pop(); // 現在のシーンが最初に入っているので削除
            int popCount = 0;
            while (sceneHistoryCopy.Count > 0)
            {
                ++popCount;
                if(nextSceneName != sceneHistoryCopy.Peek())
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
                Debug.LogError("invalid scene name:" + nextSceneName);
            }
            else
            {
                for (int i = 0; i < popCount; i++)
                {
                    sceneHistory.Pop();
                }
                string peekedNextSceneName = sceneHistory.Peek();

                // 他シーンアンロードフラグがたってたら一つだけシーンロード
                if (unloadOtherScene)
                {
                    yield return LoadOneSceneInternal(peekedNextSceneName);
                }
                else
                {
                    yield return ChangeSceneActivation(peekedNextSceneName, unloadOtherScene);
                }
            }

            // 表示中のロードUIの処理が終わるまで待機
            yield return WaitEndLoadUi();

            CleaneUpAfterChangeSceneActivation(fadeParam, afterSceneControlDelegate);
        }

        /// <summary>
        /// 現在読まれているシーン以外をすべてアンロード.
        /// </summary>
        public void UnloadAllOtherScene()
        {
            StopAllCoroutines();
            SceneSet lastSceneSet = null;
            foreach (var scene in loadedScenes) 
            {
                if(scene.sceneRoot != CurrentSceneRoot)
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
            Time.timeScale = 0;
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
        /// シーンのアクティブ状態を変更する.
        /// </summary>
        IEnumerator ChangeSceneActivation(string sceneName, bool unloadOtherScene)
        {
            // 現在のシーンをディアクティブにする.
            if (CurrentSceneRoot)
            {
                CurrentSceneRoot.SetSceneDeactive();
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
            if(CurrentSceneRoot != null)
            {
                if (CurrentSceneRoot.GetSceneName() != "demo")
                {
                    yield return new WaitForSeconds(DUMMY_LOAD_TIME);
                }
            }

            // Sceneがすでにロードされているか、ロード中なら、そのシーンをアクティブにする.
            bool isLoaded = false;
            if (loadingSubScene.Count != 0)
            {
                yield return null;  // ロード待ちの場合は終わるまで待つ
            }
            foreach (var scene in loadedScenes) 
            {
                if(scene.sceneRoot.GetSceneName() == sceneName)
                {
                    scene.sceneRoot.SetSceneActive();
                    if (unloadOtherScene)
                    {
                        var delSceneSet = loadedScenes.Find(s => CurrentSceneRoot == s.sceneRoot);
                        loadedScenes.Remove(delSceneSet);
                        var syncUnload = UnitySceneManager.UnloadSceneAsync(delSceneSet.sceneRoot.GetSceneName());
                        while(!syncUnload.isDone)
                        {
                            yield return null;
                        }
                    }
                    CurrentSceneRoot = scene.sceneRoot;
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

                if (unloadOtherScene)
                {
                    var delSceneSet = loadedScenes.Find(s => CurrentSceneRoot == s.sceneRoot);
                    loadedScenes.Remove(delSceneSet);
                    var syncUnload = UnitySceneManager.UnloadSceneAsync(delSceneSet.sceneRoot.GetSceneName());
                    while (!syncUnload.isDone)
                    {
                        yield return null;
                    }
                }

                // UNITYのバージョンアップにともない、マルチシーン機能が実装されたため親子関係による変更は廃止
                //sceneRootObj.transform.parent = gameObject.transform;
                CurrentSceneRoot = sceneRoot;
                CurrentSceneRoot.SetSceneActive();

                // 初回に必要なサブシーンをロードする
                if (sceneRoot.HasSubScene)
                {
                    yield return LoadSubSceneInternal(sceneRoot.FirstSubSceneName, false);
                }
                else
                {
                    CurrentSubSceneRoot = null;
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
        /// サブシーンロード
        /// </summary>
        IEnumerator LoadSubSceneInternal(string subSceneName, bool unloadOtherScene)
        {
            // 重複ロード防止のため1フレ待つ。既にロードされてたらやめとく
            yield return null;

            if (loadingSubScene.Count != 0)
            {
                yield return null;  // ロード待ちの場合は終わるまで待つ
            }

            if (!IsLoadedSubScene(subSceneName))
            {
                yield return LoadSceneInternal(subSceneName, true);
            }

            // サブシーンが読み終わったら、必要サブシーンを読み込む
            SubSceneRoot subSceneRoot = GetLoadedSubSceneRoot(subSceneName).GetComponent<SubSceneRoot>();
            foreach (var item in subSceneRoot.RequireSubSceneNames)
            {
                if (!IsLoadedSubScene(item))
                {
                    yield return LoadSceneInternal(item, true);
                }
            }

            // サブシーンをアクティブにする
            if (UnitySceneManager.SetActiveScene(UnitySceneManager.GetSceneByName(subSceneName)) == false)
            {
                Debug.Log("active scene fail");
            }

            if(unloadOtherScene)
            {
                // 既にロード済のサブシーンが新たなアクティブサブシーンの必要シーンに含まれていなければアンロード
                var unloadSceneList = new List<string>();
                foreach (var scene in loadedScenes)
                {
                    if (scene.sceneRoot == CurrentSceneRoot)
                    {
                        foreach (var item in scene.subSceneRoots)
                        {
                            var searchSubSceneName = item.GetSceneName();
                            if (searchSubSceneName != subSceneName)
                            {
                                if(!subSceneRoot.RequireSubSceneNames.Contains(searchSubSceneName))
                                {
                                    unloadSceneList.Add(searchSubSceneName);
                                }
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
            }
            CurrentSubSceneRoot = subSceneRoot;
        }

        /// <summary>
        /// シーンを一つだけロードして他すべてアンロード
        /// </summary>
        IEnumerator LoadOneSceneInternal(string sceneName)
        {
            // ロード済シーンは全削除して新しいシーンを開く
            loadedScenes.Clear();
            var syncLoad = UnitySceneManager.LoadSceneAsync(sceneName);
            while (!syncLoad.isDone)
            {
                yield return null;
            }
            CurrentSceneRoot = GetLoadedSceneRoot(sceneName).GetComponent<SceneRoot>();

            // 初回に必要なサブシーンをロードする
            if (CurrentSceneRoot.HasSubScene)
            {
                yield return LoadSubSceneInternal(CurrentSceneRoot.FirstSubSceneName, false);
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
            if(isSubScene)
            {
                loadingSubScene.Add(sceneName);
            }
            async.allowSceneActivation = true;
            while (async.isDone == false || async.progress < 1.0f)
            {
                yield return LibBridgeInfo.WaitForEndOfFrame;
            }
            if (isSubScene)
            {
                loadingSubScene.Remove(sceneName);
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
        public void ActiveAndApplySubScene(string subSceneName)
        {
            foreach (var scene in loadedScenes)
            {
                foreach (var item in scene.subSceneRoots)
                {
                    if (item.GetSceneName() == subSceneName)
                    {
                        ActiveAndApplySubScene(item);
                        return;
                    }
                }
            }
            if(LoadSubSceneInternalCoroutine != null) StopCoroutine(LoadSubSceneInternalCoroutine);
            LoadSubSceneInternalCoroutine = StartCoroutine(LoadSubSceneInternal(subSceneName, true));
        }
        public void ActiveAndApplySubScene(SubSceneRoot subSceneRoot)
        {
            if(subSceneRoot.GetSceneName() != UnitySceneManager.GetActiveScene().name)
            {
                if (ActiveAndApplySubSceneInternalCoroutine != null) StopCoroutine(ActiveAndApplySubSceneInternalCoroutine);
                ActiveAndApplySubSceneInternalCoroutine = StartCoroutine(ActiveAndApplySubSceneInternal(subSceneRoot));
            }
        }
        IEnumerator ActiveAndApplySubSceneInternal(SubSceneRoot subSceneRoot)
        {
            if (loadingSubScene.Count != 0)
            {
                yield return null;  // ロード待ちの場合は終わるまで待つ
            }

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

            CurrentSubSceneRoot = subSceneRoot;

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
            if(CurrentSceneRoot == null && isDirectBoot && firstDirectBoot)
            {
                firstDirectBoot = false;
                CurrentSceneRoot = sceneRoot;
                sceneHistory.Push(CurrentSceneRoot.GetSceneName());
                // どうせエディタ専用機能なのでコルーチンでサブシーンロード
                // サブシーン読み込みが先に走っていたら無視
                if (sceneRoot.HasSubScene && loadingSubScene.Count == 0)
                {
                    LoadSubSceneInternalCoroutine = StartCoroutine(LoadSubSceneInternal(sceneRoot.FirstSubSceneName, false));
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
                Debug.LogError("設定が不正です。読み込まれた一番上を親シーンかつActiveにし、親シーンの設定を確認してください。:" + subSceneRoot.name + " 親:" + subSceneRoot.ParentSceneName);
            }
        }

        /// <summary>
        /// シーン切り替え後のクリーンアップ（フェードイン等）.
        /// </summary>
        void CleaneUpAfterChangeSceneActivation(SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate)
        {
            Time.timeScale = 1;
            LoadingUIManager.Inst.HideLoadingUI(fadeParam.loadingType);
            CmnFadeManager.Inst.StartFadeIn(EndFadeInCallBack, fadeParam.fadeInTime, fadeParam.fadeType, fadeParam.fadeColor);
            sceneUI.ChangeCommonSceneUI(CurrentSceneRoot.SceneUiParam, CurrentSceneRoot.SceneBgKind);
            sceneUI.ChangeSceneTitleLabel(CurrentSceneRoot.SceneNameLocalizeID);
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
        /// ロードが終了しているサブシーンルートを取得する.
        /// </summary>
        public IReadOnlyList<SubSceneRoot> GetCurrentSubSceneRoots()
        {
            foreach (var scene in loadedScenes)
            {
                if(scene.sceneRoot == CurrentSceneRoot)
                {
                    return scene.subSceneRoots;
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

        /// <summary>
        /// ロードUIの終了を待機
        /// </summary>
        IEnumerator WaitEndLoadUi()
        {
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