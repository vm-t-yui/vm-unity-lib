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
            if(other.tag == TagName.Player)
            {
                // サブシーンを切り替えて確定待ちをする
                if(SubSceneActiveChangingCnt == 0)
                {
                    PrevSubSceneRoot = SceneManager.Instance?.CurrentSubSceneRoot;
                    NextSubSceneRoot = TargetSubSceneRoot;
                    SceneManager.Instance?.ActiveAndApplySubScene(NextSubSceneRoot);
                }
                ++SubSceneActiveChangingCnt;
                if(SubSceneActiveChangingCnt > 2)
                {
                    Debug.LogError("SubSceneActivateTriggerが３つ以上重なっています。SubSceneActivateTriggerは２つが重なっており、" +
                    "これから読み込む先のトリガーが先にヒットする必要があります");
                }
            }
        }

        /// <summary>
        /// 抜けた時
        /// </summary>
        private void OnTriggerExit(Collider other)
        {
            if (other.tag == TagName.Player)
            {
                if(SubSceneActiveChangingCnt == 1)
                {
                    // Exit呼ばれたのがNext側なら確定。何もしない
                    // Exit呼ばれたのがPrev側ならキャンセル読み込み
                    if (TargetSubSceneRoot == NextSubSceneRoot) // targetがnext=prev側
                    {
                        SceneManager.Instance?.ActiveAndApplySubScene(PrevSubSceneRoot);
                    }
                }
                else if (SubSceneActiveChangingCnt == 2)
                {
                    // Exit呼ばれたのがNext側ならキャンセルしてカウント減算
                    if(PrevSubSceneRoot == TargetSubSceneRoot)
                    {
                        SceneManager.Instance?.ActiveAndApplySubScene(PrevSubSceneRoot);
                    }
                    // Exit呼ばれたのがPrev側なら次の読み込みが確定しているのでNextを現在のものに更新してキャンセルに備える
                    else
                    {
                        NextSubSceneRoot = TargetSubSceneRoot;
                    }
                }
                --SubSceneActiveChangingCnt;
            }
        }
    }
}