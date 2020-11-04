using UnityEngine;

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
        /// ロード開始
        /// </summary>
        public virtual void OnStartLoad() { }
    }
}
