/******************************************************************************/
/*!    \brief  全プログラムの共通処理.
*******************************************************************************/

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VMUnityLib
{
    public sealed class CommonProgramObj : MonoBehaviour
    {
#if UNITY_EDITOR
        private int prevScreenW;
        private int prevScreenH;
        EditorWindow gameview;
#endif

        // 音声の遅延フレーム数.
        static public int SoundLatency { private set; get; }
        
        /// <summary>
        /// 自身の生成前に呼ばれる関数。生成するしない関係なしに呼ばれる.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            if (SceneManager.Instance == null)
            {
                GameObject commonProgramObjPrefab = Resources.Load<GameObject>("CommonProgramObj");
                if (commonProgramObjPrefab == null)
                {
                    Debug.LogError("commonProgramObjPrefabのロードに失敗");
                }
                GameObject.Instantiate(commonProgramObjPrefab);
            }
        }

        /// <summary>
        /// 初期化.
        /// </summary>
        private void Start()
        {
#if UNITY_EDITOR
            gameview = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
#endif
            // 音の遅延の計測を開始する.
            SoundLatency = PlayerPrefs.GetInt("SoundLatency", -1);
            if (SoundLatency < 0)
            {
#if USE_CRI
                CriAtomExLatencyEstimator.InitializeModule();
                StartCoroutine(CheckSoundLatency());
#else
                SoundLatency = 0;
#endif
            }
        }

        /// <summary>
        /// GUI.
        /// </summary>
        private void OnGUI()
        {
#if UNITY_EDITOR
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
#endif
        }

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
