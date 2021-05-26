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
        // サブシーンのアクティブ状態を切り替え中かどうか
        static int          SubSceneActiveChangingCnt{ get; set; }
        // 切り替えが確定する直前・直後のサブシーンルート
        static string PrevSubSceneName{ get; set; }
        static string NextSubSceneName { get; set; }

        string targetSubSceneName;

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
            SubSceneRoot targetSubSceneRoot = null;
            foreach (var item in rootObjects)
            {
                targetSubSceneRoot = item.GetComponent<SubSceneRoot>();
                if (targetSubSceneRoot)
                    break;
            }
            if(!targetSubSceneRoot)
            {
                Debug.LogError("サブシーンではないシーンにトリガーが存在します (" + gameObject.name + ") in "+ gameObject.scene.name, gameObject);
            }
            else
            {
                targetSubSceneName = targetSubSceneRoot.GetSceneName();
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
                    PrevSubSceneName = SceneManager.Instance?.CurrentPlayerSubSceneName;
                    NextSubSceneName = targetSubSceneName;
                    //Debug.Log("OnTriggerEnter and(next) apply:" + NextSubSceneName, gameObject);
                    SceneManager.Instance?.ActiveAndApplySubScene(NextSubSceneName, false);
                }
                ++SubSceneActiveChangingCnt;
                //Debug.Log("SubSceneActiveChangingCnt add:" + SubSceneActiveChangingCnt, gameObject);
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
                    if (targetSubSceneName == NextSubSceneName)
                    {
                        var prevSceneName = PrevSubSceneName;
                        //Debug.Log("OnTriggerExit(cancel) apply:" + prevSceneName, gameObject);
                        SceneManager.Instance?.ActiveAndApplySubScene(prevSceneName, true);
                    }
                    else
                    {
                        // シーン移動が確定するのでプレイヤーのいるシーンを確定
                        SceneManager.Instance?.UpdatePlayerSubScne(NextSubSceneName);
                    }
                }
                --SubSceneActiveChangingCnt;
                //Debug.Log("SubSceneActiveChangingCnt sub:" + SubSceneActiveChangingCnt, gameObject);
            }
        }
    }
}