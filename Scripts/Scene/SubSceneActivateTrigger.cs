using UnityEngine;
#if DEBUG && UNITY_EDITOR
using UnityEditor;
#endif
using Sirenix.OdinInspector;

/// <summary>
/// 自身が所属するサブシーンをアクティブ化するトリガー
/// 隣のサブシーンとの切り替え用のトリガーは重なっている必要があり、これから読み込む側のトリガーが先にヒットする必要がある
/// </summary>
namespace VMUnityLib
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class SubSceneActivateTrigger : MonoBehaviour
    {
#if DEBUG && UNITY_EDITOR
        private void OnValidate()
        {
            var rigid = GetComponent<Rigidbody>();
            bool dirty = false;
            if(rigid == null)
            {
                rigid = gameObject.AddComponent<Rigidbody>();
                dirty = true;
            }
            if(!rigid.isKinematic)
            {
                rigid.isKinematic = true;
                dirty = true;
            }
            var box = GetComponent<BoxCollider>();
            if (box == null)
            {
                box = gameObject.AddComponent<BoxCollider>();
                dirty = true;
            }
            if (!box.isTrigger)
            {
                box.isTrigger = true;
                dirty = true;
            }
            if(dirty)
            {
                EditorUtility.SetDirty(this);
                Undo.RecordObject(this, "SubSceneActivateTrigger set ");
            }
        }

        public void DebugSetTargetSceneName(string target) { targetSubSceneName = target; }
#endif
        // サブシーンのアクティブ状態を切り替え中かどうか
        public static bool IsSubsceneTriggerWorking { get { return SubSceneActiveChangingCnt != 0; } }
        static int  SubSceneActiveChangingCnt{ get; set; }
        // 切り替えが確定する直前・直後のサブシーンルート
        static string PrevSubSceneName{ get; set; }
        static string NextSubSceneName { get; set; }

        [SerializeField, LabelText("Rootシーンに配置")]
        bool isOnRoot = false;
        public bool IsOnRoot => isOnRoot;

        [SerializeField, LabelText("対象シーン名"), SceneName, EnableIf("IsOnRoot")]
        string targetSubSceneName = default;

        /// <summary>
        /// 開始前
        /// </summary>
        private void Awake()
        {
            gameObject.layer = LayerName.IgnoreRaycast;
        }

        /// <summary>
        /// Start
        /// </summary>
        private void Start()
        {
            // サブシーンに配置されているものは自分の目標シーン名自動取得
            if (!IsOnRoot)
            {
                var rootObjects = gameObject.scene.GetRootGameObjects();
                SubSceneRoot targetSubSceneRoot = null;
                foreach (var item in rootObjects)
                {
                    targetSubSceneRoot = item.GetComponent<SubSceneRoot>();
                    if (targetSubSceneRoot)
                        break;
                }
                if (!targetSubSceneRoot)
                {
                    Debug.LogError("サブシーンではないシーンにトリガーが存在します (" + gameObject.name + ") in " + gameObject.scene.name, gameObject);
                }
                else
                {
                    targetSubSceneName = targetSubSceneRoot.GetSceneName();
                }
            }
        }

        /// <summary>
        /// トリガーに入った時
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if(other.tag == TagName.MainCamera)
            {
                // 初回のEnterでNextPrevを決定する
                // サブシーンを切り替えて確定待ちをする
                Debug.Log("SubSceneActiveChangingCnt" + SubSceneActiveChangingCnt + " in:" + GetHierarchyPath(gameObject), gameObject);
                if(SubSceneActiveChangingCnt == 0)
                {
                    PrevSubSceneName = SceneManager.Instance?.CurrentPlayerSubSceneName;
                    NextSubSceneName = targetSubSceneName;
                    Debug.Log("★OnTriggerEnter and(next) apply:" + NextSubSceneName, gameObject);
                    SceneManager.Instance?.ActiveAndApplySubScene(NextSubSceneName, false);
                }
                ++SubSceneActiveChangingCnt;
            }
        }

        /// <summary>
        /// 抜けた時
        /// </summary>
        private void OnTriggerExit(Collider other)
        {
            if (other.tag == TagName.MainCamera)
            {
                Debug.Log("SubSceneActiveChangingCnt" + SubSceneActiveChangingCnt + " out:" + GetHierarchyPath(gameObject), gameObject);
                // 重なっているものが一つもなくなった時、最初にEnterした箇所からExitしてたらキャンセル
                // それ以外は最後に抜けたところを適用確定(最初のEnterの時に適用されているから何もしない
                if (SubSceneActiveChangingCnt == 1)
                {
                    // targetがNext=自分シーン側
                    if (targetSubSceneName == NextSubSceneName)
                    {
                        var prevSceneName = PrevSubSceneName;
                        Debug.Log("★OnTriggerExit(cancel) apply:" + prevSceneName, gameObject);
                        SceneManager.Instance?.ActiveAndApplySubScene(prevSceneName, true);
                    }
                    else
                    {
                        // シーン移動が確定するのでプレイヤーのいるシーンを確定
                        Debug.Log("★OnTriggerExit(decide) apply:" + NextSubSceneName, gameObject);
                        SceneManager.Instance?.UpdatePlayerSubScne(NextSubSceneName);
                    }
                }
                --SubSceneActiveChangingCnt;
            }
        }

        /// <summary>
        /// ヒエラルキーに応じたパスを取得する
        /// </summary>
        static string GetHierarchyPath(GameObject target)
        {
            string path = "";
            Transform current = target.transform;
            while (current != null)
            {
                // 同じ階層に同名のオブジェクトがある場合があるので、それを回避する
                int index = current.GetSiblingIndex();
                path = "/" + current.name + "[" + index + "]" + path;
                current = current.parent;
            }

            return "/" + target.scene.name + path;
        }
    }
}