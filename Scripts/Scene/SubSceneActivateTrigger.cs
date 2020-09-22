using UnityEngine;

/// <summary>
/// 自身が所属するサブシーンをアクティブ化するトリガー
/// 隣のサブシーンとの切り替え用のトリガーは重なっている必要があり、これから読み込む側のトリガーが先にヒットする必要がある
/// </summary>
namespace VMUnityLib
{
    [RequireComponent(typeof(BoxCollider))]
    public class SubSceneActivateTrigger : MonoBehaviour
    {
        public SubSceneRoot TargetSubSceneRoot { get; private set; }

        // サブシーンのアクティブ状態を切り替え中かどうか
        static int          SubSceneActiveChangingCnt{ get; set; }
        // 切り替えが確定する直前・直後のサブシーンルート
        static SubSceneRoot PrevSubSceneRoot{ get; set; }
        static SubSceneRoot NextSubSceneRoot { get; set; }

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
            // サブシーンルート取得
            var rootObjects = gameObject.scene.GetRootGameObjects();
            foreach (var item in rootObjects)
            {
                TargetSubSceneRoot = item.GetComponent<SubSceneRoot>();
                if (TargetSubSceneRoot)
                    break;
            }
            if(!TargetSubSceneRoot)
            {
                Debug.LogError("サブシーンではないシーンにトリガーが存在します (" + gameObject.name + ") in "+ gameObject.scene.name);
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
                if(SubSceneActiveChangingCnt == 0)
                {
                    PrevSubSceneRoot = SceneManager.Instance?.CurrentSubSceneRoot;
                    NextSubSceneRoot = TargetSubSceneRoot;
                    SceneManager.Instance?.ActiveAndApplySubScene(NextSubSceneRoot.GetSceneName());
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
                // 重なっているものが一つもなくなった時、最初にEnterした箇所からExitしてたらキャンセル
                // それ以外は最後に抜けたところを適用確定(最初のEnterの時に適用されているから何もしない
                if(SubSceneActiveChangingCnt == 1)
                {
                    // targetがNext=自分シーン側
                    if (TargetSubSceneRoot == NextSubSceneRoot)
                    {
                        SceneManager.Instance?.ActiveAndApplySubScene(PrevSubSceneRoot.GetSceneName());
                    }
                }
                --SubSceneActiveChangingCnt;
            }
        }
    }
}