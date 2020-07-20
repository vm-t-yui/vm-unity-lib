/******************************************************************************/
/*!    \brief  シングルトンのMonoBehaviourジェネリック.
*******************************************************************************/

using UnityEditor;
using UnityEngine;
namespace VMUnityLib
{
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        static T instance;

        /// <summary>
        /// Awake時にインスタンス設定
        /// </summary>
        virtual protected void Awake()
        {
            // 他のGameObjectにアタッチされているか調べる.
            // アタッチされている場合は破棄する.
            if(instance == null)
            {
                instance = GetComponent<T>();
            }
            else if (this != instance)
            {
                Destroy(gameObject);
                Debug.LogError(
                    typeof(T) +
                    " は既に他のGameObjectにアタッチされているため、コンポーネントを破棄しました." +
                    " アタッチされているGameObjectは " + instance.gameObject.name + " です.");
            }
        }

        /// <summary>
        /// 削除時にnull入れる
        /// </summary>
        private void OnDestroy()
        {
            instance = null;
        }

        public static T Inst
        {
            get
            {
                // もしAwake以前にインスタンス取得しようとしていたら警告
                if (instance == null)
                {
#if UNITY_EDITOR && DEBUG
                    // 警告は再生中のみ
                    if (EditorApplication.isPlaying)
                    {
                        Debug.LogWarning("Awakeが呼ばれる前にInstにアクセスしようとしました。" +
                        "^\nScriptExecutionOrderを確認してください");
                    }
#endif
                    // 全検索
                    instance = (T)FindObjectOfType(typeof(T));

                    if (instance == null)
                    {
                        Debug.LogError(typeof(T) + "全検索をかけても見つかりませんでした");
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// インスタンスチェック。Instでチェックするとtype全検索かけてしまう
        /// </summary>
        public static bool IsNullInstance { get { return (instance == null); } }
    }
}
