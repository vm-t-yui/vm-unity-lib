using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VMUnityLib
{
    /// <summary>
    /// プログラム用プレハブ。ENABLE_AB_LOADでAssetBundleからロードする場合、該当ファイルをExcludeResources.jsonでビルド時除外することを推奨
    /// </summary>
    public sealed class CommonProgramObj : MonoBehaviour
    {
        const string prefabName = "CommonProgramObj";
#if UNITY_EDITOR
        int prevScreenW;
        int prevScreenH;
        EditorWindow gameview;
        const string prefabPath = "Assets/MyGameAssets/LibBridge/Prefabs/CommonProgramObj.prefab";
#endif

        // 音声の遅延フレーム数.
        static public int SoundLatency { set; get; }

        /// <summary>
        /// 自身の生成前に呼ばれる関数。生成するしない関係なしに呼ばれる.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            GameObject prefab = null;
#if ENABLE_AB_LOAD && !UNITY_EDITOR
            // アセットバンドルからプレハブをロード
            var ab = AssetBundleLoader.LoadedAssetBundlesByName["commonprefab"];
            prefab = ab.LoadAsset<GameObject>(prefabName);
#else
            // Resourcesからロードする
            Object obj = Resources.Load(prefabName);
            prefab = (GameObject)obj;
#endif

            if (prefab == null)
            {
                Debug.LogError(prefabName + "のロードに失敗 prefab:" + prefab);
#if UNITY_EDITOR
                prefab = (GameObject)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
#endif
            }
            else
            {
                Debug.Log("CommonProgramObj loaded.");
            }
            var instantiated = Instantiate(prefab);
            DontDestroyOnLoad(instantiated);
        }

        /// <summary>
        /// 初期化.
        /// </summary>
        void Start()
        {
#if UNITY_EDITOR
            gameview = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
#endif
            // 音の遅延の計測を開始する.
#if USE_CRI
            SoundLatency = PlayerPrefs.GetInt("SoundLatency", -1);
            CriAtomExLatencyEstimator.InitializeModule();
            StartCoroutine(CheckSoundLatency());
#else
            SoundLatency = 0;
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// GUI.
        /// </summary>
        void OnGUI()
        {
            if (prevScreenW != Screen.width || prevScreenH != Screen.height)
            {
                GameWindowSize.ReCalc(LibBridgeInfo.FIXED_SCREEN_WI, LibBridgeInfo.FIXED_SCREEN_HI);
                foreach (GUIStyle style in GUI.skin)
                {
                    style.fontSize = (int)(20 * GameWindowSize.GameScreenScale);
                }
            }
            prevScreenW = (int)gameview.position.width;
            prevScreenH = (int)gameview.position.height;
        }
#endif

#if USE_CRI
        /// <summary>
        /// 遅延サウンドチェック.
        /// </summary>
        /// <returns></returns>
        IEnumerator CheckSoundLatency()
        {
            WaitForEndOfFrame wait =  new WaitForEndOfFrame();
            bool loop = true;
            while(loop)
            {
                CriAtomExLatencyEstimator.EstimatorInfo info = CriAtomExLatencyEstimator.GetCurrentInfo();
                switch (info.status)
                {
                    case CriAtomExLatencyEstimator.Status.Done:
                        loop = false;
                        SoundLatency = (int)info.estimated_latency;
                        PlayerPrefs.SetInt("SoundLatency", SoundLatency);
                        break;

                    case CriAtomExLatencyEstimator.Status.Processing:
                        yield return wait;
                        break;

                    case CriAtomExLatencyEstimator.Status.Error:
                        Debug.LogError("Sound latency check fail.");
                        loop = false;
                        SoundLatency = 0;
                        break;

                    case CriAtomExLatencyEstimator.Status.Stop:
                        loop = false;
                        SoundLatency = 0;
                        break;
                }
            }
        }
#endif
    }
}
