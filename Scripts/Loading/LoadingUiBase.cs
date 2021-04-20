using UnityEngine;
using System.Collections;

namespace VMUnityLib
{
    /// <summary>
    /// ロードUIのベースクラス
    /// NOTE : t-mitsumaru ロードUIのオブジェクトには、このクラスを必ずアタッチしてください。
    ///                    特定のロードUIで固有の処理が必要な場合はこのクラスを継承してください。
    /// </summary>
    public class LoadingUiBase : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("ロードUIの処理で次のシーンへ移行できるタイミングになったときにisEndをtrueにしてください。\n" +
                 "trueになったら自動でロードUIが閉じられ、次のシーンへ移行します。")]
        protected bool isEnd = false;
        public bool IsEnd => isEnd;

        /// <summary>
        /// 有効化されたとき
        /// </summary>
        virtual protected void OnEnable()
        {
            isEnd = false;
        }

        /// <summary>
        /// ロード開始前処理
        /// </summary>
        public virtual IEnumerator BeforeStartLoadProcess()
        {
            yield break;
        }

        /// <summary>
        /// ロード開始直後処理
        /// </summary>
        public virtual IEnumerator AfterStartLoadWaitProcess()
        {
            yield break;
        }
    }
}
