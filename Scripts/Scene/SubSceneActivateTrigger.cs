using UnityEngine;

/// <summary>
/// 自身が所属するサブシーンをアクティブ化するトリガー
/// </summary>
/// 
namespace VMUnityLib
{
    [RequireComponent(typeof(BoxCollider))]
    public class SubSceneActivateTrigger : MonoBehaviour
    {
        public SubSceneRoot TargetSubSceneRoot { get; private set; }

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
                SceneManager.Instance?.ActiveAndApplySubScene(TargetSubSceneRoot);
            }
        }
    }
}