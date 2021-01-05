/******************************************************************************/
/*!    \brief  シーンマネージャ.
*******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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

        SceneRoot currentSceneRoot = null;                             // 現在アクティブなシーンルート.
        List<SceneRoot> loadedSceneRootList = new List<SceneRoot>();   // ロード済みのシーンルート.
        Stack<string> sceneHistory = new Stack<string>();              // シーンの遷移ヒストリ.

        Stack<KeyValuePair<string, int>> sceneAnchor = new Stack<KeyValuePair<string, int>>();    // シーンのアンカー.

        bool isFadeWaiting = false;             // フェードが終わったかどうか
        CommonSceneUI sceneUI = null;           // 共通シーンUI
        bool isSubscribeUnloadEvent = false;    // アンロードイベントを登録したかどうか

        [SceneNameAttribute, SerializeField] string firstSceneName = default;       // 最初のシーンの名前
        [SceneNameAttribute, SerializeField] string debugFirstSceneName = default;  // デモの最初のシーンの名前
        [SerializeField] bool isDebug = true;   // デバックさせるかどうか

        public static SceneManager Instance { get; set; } = default;   // インスタンス

        // シーンチェンジ時のフェードのパラメータ.
        public struct SceneChangeFadeParam
        {
            public float fadeOutTime;                       // フェードイン所要時間
            public float fadeInTime;                        // フェードアウト所要時間
            public CmnFadeManager.FadeType fadeType;        // フェードのタイプ
            public Color fadeColor;                         // フェードの色
            public LibBridgeInfo.LoadingType loadingType;   // ロードのタイプ

            // パラメーター設定
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
        /// 開始処理
        /// </summary>
        void Start()
        {
            StartCoroutine(SceneStartColutine());
        }
        IEnumerator SceneStartColutine()
        {
            // 共通シーンUIとフェードのパラメータの初期設定
            sceneUI = CommonSceneUI.Inst;
            SceneChangeFadeParam noTimeFade = new SceneChangeFadeParam(0, 0, CmnFadeManager.FadeType.FADE_TIMEONLY, new Color(0, 0, 0, 0), LibBridgeInfo.LoadingType.COMMON);

            //　準備が整うまでシーン開始は遅延.
            yield return LibBridgeInfo.WaitForEndOfFrame;
            yield return LibBridgeInfo.WaitForEndOfFrame;

            // 準備が整ったらシーン変更
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
        /// 起動処理
        /// </summary>
        void Awake()
        {
            // インスタンスがない場合はこのシーンマネージャーを追加、既にある場合はこのシーンマネージャーを破壊する
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
        /// <param name="SceneName">シーン名.</param>
        /// <param name="fadeTime">フェードパラメータ.</param>
        /// <param name="pushAnchor">シーンまでのアンカー.</param>
        public void PushScene(string SceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate = null, string pushAnchor = null)
        {
            // フェードアウト開始
            isFadeWaiting = true;
            CmnFadeManager.Inst.StartFadeOut(EndFadeOutCallBack, fadeParam.fadeOutTime, fadeParam.fadeType, fadeParam.fadeColor);

            // シーンプッシュ開始
            StartCoroutine(PushSceneInternal(SceneName, fadeParam, afterSceneControlDelegate, pushAnchor));
        }

        /// <summary>
        /// シーンプッシュのコルーチン
        /// </summary>
        /// <param name="SceneName">シーン名</param>
        /// <param name="fadeParam">フェードパラメータ</param>
        /// <param name="pushAnchor">シーンまでのアンカー</param>
        IEnumerator PushSceneInternal(string SceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate, string pushAnchor)
        {
            // フェードアウトが完了するまで待機
            while (isFadeWaiting)
            {
                yield return null;
            }

            // ロード画面のUI表示(表示のために１フレーム待機)
            LoadingUIManager.Inst.ShowLoadingUI(fadeParam.loadingType);
            yield return null;

            // シーンのアクティブ状態を変更
            yield return StartCoroutine(ChangeSceneActivation(SceneName));

            // シーンの遷移ヒストリに登録
            sceneHistory.Push(SceneName);

            Debug.Log(pushAnchor);
            // シーンまでのアンカーの登録
            if (pushAnchor != null)
            {
                // シーンまでのアンカーが見つかったかどうかのフラグ
                bool find = false;

                // シーンまでのアンカーを探す
                foreach (KeyValuePair<string, int> pair in sceneAnchor)
                {
                    if (pair.Key == pushAnchor)
                    {
                        find = true;
                        break;
                    }
                }
                // 見つからなかったらアンカーを登録、同じものがz見つかったらエラー
                if (find == false)
                {
                    sceneAnchor.Push(new KeyValuePair<string, int>(pushAnchor, sceneHistory.Count - 1));
                }
                else
                {
                    Logger.Error("same name anchor found");
                }

            }

            // シーン切り替え後のクリーンアップ
            CleaneUpAfterChangeSceneActivation(fadeParam, afterSceneControlDelegate);
        }

        /// <summary>
        /// シーン変更.
        /// </summary>
        /// <param name="SceneName">シーン名.</param>
        /// <param name="fadeTime">フェードパラメータ.</param>
        public void ChangeScene(string SceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate = null)
        {
            // フェードアウト開始
            isFadeWaiting = true;
            CmnFadeManager.Inst.StartFadeOut(EndFadeOutCallBack, fadeParam.fadeOutTime, fadeParam.fadeType, fadeParam.fadeColor);

            // シーン変更開始
            StartCoroutine(ChangeSceneInternal(SceneName, fadeParam, afterSceneControlDelegate));
        }

        /// <summary>
        /// シーン変更のコルーチン
        /// </summary>
        /// <param name="SceneName">シーン名</param>
        /// <param name="fadeParam">フェードパラメータ</param>
        IEnumerator ChangeSceneInternal(string SceneName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate)
        {
            // フェードアウトが完了するまで待機
            while (isFadeWaiting)
            {
                yield return null;
            }

            // ロード画面のUI表示(表示のために１フレーム待機)
            LoadingUIManager.Inst.ShowLoadingUI(fadeParam.loadingType);
            yield return null;

            // シーンの遷移ヒストリが登録されているなら
            if (sceneHistory.Count > 0)
            {
                // シーンの遷移ヒストリから一番先頭を削除してから、登録
                sceneHistory.Pop();
                sceneHistory.Push(SceneName);

                // シーンのアクティブ状態を変更
                yield return StartCoroutine(ChangeSceneActivation(SceneName));
            }

            // シーン切り替え後のクリーンアップ
            CleaneUpAfterChangeSceneActivation(fadeParam, afterSceneControlDelegate);
        }

        /// <summary>
        /// シーンポップ.
        /// </summary>
        /// <param name="fadeTime">フェードパラメータ.</param>
        public void PopScene(SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate = null)
        {
            // フェードアウト開始
            isFadeWaiting = true;
            CmnFadeManager.Inst.StartFadeOut(EndFadeOutCallBack, fadeParam.fadeOutTime, fadeParam.fadeType, fadeParam.fadeColor);

            // シーンポップ開始
            StartCoroutine(PopSceneInternal(fadeParam, afterSceneControlDelegate));
        }

        /// <summary>
        /// シーンポップのコルーチン
        /// </summary>
        /// <param name="fadeParam">フェードパラメータ</param>
        IEnumerator PopSceneInternal(SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate)
        {
            // フェードアウトが完了するまで待機
            while (isFadeWaiting)
            {
                yield return null;
            }

            // ロード画面のUI表示(表示のために１フレーム待機)
            LoadingUIManager.Inst.ShowLoadingUI(fadeParam.loadingType);
            yield return null;

            // シーンの遷移ヒストリが登録されているなら
            if (sceneHistory.Count > 0)
            {
                // シーンの遷移ヒストリから一番先頭を削除し、その後の一番先頭のシーンに変更
                sceneHistory.Pop();
                string SceneName = sceneHistory.Peek();
                yield return StartCoroutine(ChangeSceneActivation(SceneName));
            }

            // シーン切り替え後のクリーンアップ
            CleaneUpAfterChangeSceneActivation(fadeParam, afterSceneControlDelegate);
        }

        /// <summary>
        /// 指定アンカーまでシーンポップ.
        /// </summary>
        /// <param name="anchorName">ポップ先のアンカー名.</param>
        /// <param name="fadeParam">フェードパラメータ.</param>
        public void PopSceneToAnchor(string anchorName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate = null)
        {
            // フェードアウト開始
            isFadeWaiting = true;

            // 指定アンカーが見つかったかどうかのフラグ
            bool bFound = false;

            // 指定アンカーを探す
            foreach (KeyValuePair<string, int> pair in sceneAnchor)
            {
                // 見つかったらシーンポップ開始
                if (pair.Key == anchorName)
                {
                    CmnFadeManager.Inst.StartFadeOut(EndFadeOutCallBack, fadeParam.fadeOutTime, fadeParam.fadeType, fadeParam.fadeColor);
                    StartCoroutine(PopSceneToAnchorInternal(anchorName, fadeParam, afterSceneControlDelegate));
                    bFound = true;
                    break;
                }
            }
            // アンカーが無ければ警告出して無視
            if (bFound == false)
            {
                Logger.Warn("Anchor not found.:" + anchorName);
            }
        }

        /// <summary>
        /// 指定アンカーまでシーンポップのコルーチン
        /// </summary>
        /// <param name="anchorName">ポップ先のアンカー名</param>
        /// <param name="fadeParam">フェードパラメータ</param>
        IEnumerator PopSceneToAnchorInternal(string anchorName, SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate)
        {
            // フェードアウトが完了するまで待機
            while (isFadeWaiting)
            {
                yield return null;
            }

            // ロード画面のUI表示(表示のために１フレーム待機)
            LoadingUIManager.Inst.ShowLoadingUI(fadeParam.loadingType);
            yield return null;

            // 同名アンカー
            int popAnchorCount = -1;

            // 同名アンカー探す
            foreach (KeyValuePair<string, int> pair in sceneAnchor)
            {
                if (pair.Key == anchorName)
                {
                    popAnchorCount = pair.Value;
                    break;
                }
            }

            // 同名アンカーが見つかったらそこまでポップ
            if (popAnchorCount >= 0)
            {
                while (sceneHistory.Count - 1 > popAnchorCount)
                {
                    sceneHistory.Pop();
                }
                string nextSceneName = sceneHistory.Peek();
                yield return StartCoroutine(ChangeSceneActivation(nextSceneName));
            }
            // 見つからなかったらエラー
            else
            {
                Logger.Error("invalid anchor name:" + anchorName);
            }

            // シーン切り替え後のクリーンアップ
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
                if (root != currentSceneRoot)
                {
#if UNITY_5_5_OR_NEWER
                    UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(root.GetSceneName());
#else
                    UnityEngine.SceneManagement.SceneManager.UnloadScene(root.GetSceneName());
#endif
                    Destroy(root.gameObject);
                }
            }
            loadedSceneRootList = new List<SceneRoot>();
            loadedSceneRootList.Add(currentSceneRoot);
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
                Destroy(root.gameObject);
            }
            loadedSceneRootList = new List<SceneRoot>();
            sceneAnchor = new Stack<KeyValuePair<string, int>>();
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
            EventManager.Inst.InvokeEvent(SubjectType.EndFadeIn);
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
            if (currentSceneRoot != null)
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
                if (root.GetSceneName() == SceneName)
                {
                    root.SetSceneActive();
                    currentSceneRoot = root;
                    isLoaded = true;
#if USE_POOL_MANAGER
                    // プールマネージャー初期化.
                    foreach (KeyValuePair<string, SpawnPool> pool in PoolManager.Pools)
                    {
                        pool.Value.DespawnAll();
                    }
#endif

                    break;
                }
            }

            //ロードされていなかったらAddiveLoadする.
            if (isLoaded == false)
            {
                // シーンが読み込まれるまでの進行度を取得
                AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(SceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);

                // シーンの有効化を許可
                async.allowSceneActivation = true;

                // 動作が終了していなかったら待機
                while (async.isDone == false || async.progress < 1.0f)
                {
                    yield return LibBridgeInfo.WaitForEndOfFrame;
                }

                // ロードが終了しているシーンルートを取得
                GameObject sceneRootObj = GetLoadedSceneRoot(SCENE_ROOT_NAME_HEADER + SceneName);
                // 待機時間
                int waitCnt = 0;

                // シーンルートが取得できていない場合は一定時間まで探し続ける
                while (sceneRootObj == null && waitCnt < 120)
                {
                    ++waitCnt;
                    sceneRootObj = GetLoadedSceneRoot(SCENE_ROOT_NAME_HEADER + SceneName);
                    yield return LibBridgeInfo.WaitForEndOfFrame;
                }

                // まだシーンルートが取得できていない場合
                if (sceneRootObj == null)
                {
                    // エラーを出してロード済シーンルートをダンプ
                    Debug.LogError("Scene root not found:" + SCENE_ROOT_NAME_HEADER + SceneName + " waited" + waitCnt + "frame.");
                    DumpLoadedSceneRoot();

                    // 再度ロードが終了しているシーンルートを取得
                    UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(SceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
                    sceneRootObj = GetLoadedSceneRoot(SCENE_ROOT_NAME_HEADER + SceneName);

                    // それでもなかったらエラーを出してロード済シーンルートをダンプ
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
            if (loadedSceneRootList.Contains(sceneRoot) == false)
            {
                loadedSceneRootList.Add(sceneRoot);
            }
        }

        /// <summary>
        /// シーン切り替え後のクリーンアップ（フェードイン等）.
        /// </summary>
        void CleaneUpAfterChangeSceneActivation(SceneChangeFadeParam fadeParam, AfterSceneControlDelegate afterSceneControlDelegate)
        {
            // ロード画面のテキストを隠してフェードイン
            LoadingUIManager.Inst.HideLoadingUI(fadeParam.loadingType);
            CmnFadeManager.Inst.StartFadeIn(EndFadeInCallBack, fadeParam.fadeInTime, fadeParam.fadeType, fadeParam.fadeColor);

            // シーンUIとタイトルの切り替え
            sceneUI.ChangeCommonSceneUI(currentSceneRoot.SceneUiParam, currentSceneRoot.SceneBgKind);
            sceneUI.ChangeSceneTitleLabel(currentSceneRoot.SceneNameLocalizeID);

            if (afterSceneControlDelegate != null)
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
            foreach (SceneRoot root in loadedSceneRootList)
            {
                if (root.gameObject.name == rootName)
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


        public void SubscribeSceneUnloadEvent()
        {
            if(!isSubscribeUnloadEvent)
            {
                isSubscribeUnloadEvent = true;
            EventManager.Inst.Subscribe(SubjectType.EndFadeIn,Unit => UnloadSceneBack());
            }
        }


        void UnloadSceneBack()
        {
            var root = loadedSceneRootList[loadedSceneRootList.Count - 2];

            if (root != currentSceneRoot && root.GetSceneName() != "demo")
            {
#if UNITY_5_5_OR_NEWER
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(root.GetSceneName());
#else
                    UnityEngine.SceneManagement.SceneManager.UnloadScene(root.GetSceneName());
#endif
                Destroy(root.gameObject);
                loadedSceneRootList.Remove(root);
            }
        }
    }
}