/******************************************************************************/
/*!    \brief  シングルトンのMonoBehaviourジェネリック.
*******************************************************************************/

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
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
                string myPath = GetHierarchyPath(gameObject);
                string originPath = GetHierarchyPath(instance.gameObject);
                Debug.LogError(
                    typeof(T) +
                    "は既に他のGameObjectにアタッチされているため、自身を破棄しました。" +
                    "\nmyPath:" + myPath +
                    "\noriginPath:" + originPath);
                Destroy(gameObject);
            }
        }
        static string GetHierarchyPath(GameObject targetObj)
        {
            List<GameObject> objPath = new List<GameObject>();
            objPath.Add(targetObj);
            for (int i = 0; objPath[i].transform.parent != null; i++)
                objPath.Add(objPath[i].transform.parent.gameObject);
            string path = objPath[objPath.Count - 1].gameObject.name;
            for (int i = objPath.Count - 2; i >= 0; i--)
                path += "/" + objPath[i].gameObject.name;

            return path;
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
                        Debug.LogWarning("Awakeが呼ばれる前にInstにアクセスしようとしました。\n"
                        + typeof(T).ToString() + "のScriptExecutionOrderを確認してください.\n");
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
