using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// ロードUIのベースクラス
/// </summary>
[TypeInfoBox("ロードUIのオブジェクトには、このクラスを必ずアタッチしてください。\n" +
             "特定のロードUIで固有の処理が必要な場合はこのクラスを継承してください。")]
public class LoadingUiBase : MonoBehaviour
{
    [SerializeField,LabelText("UIの処理が終了したかどうか")]
    [InfoBox("ロードUIの処理で次のシーンへ移行できるタイミングになったときにisEndをtrueにしてください。\n" +
             "trueになったら自動でロードUIが閉じられ、次のシーンへ移行します。")]
    protected bool isEnd = false;
    public bool IsEnd => isEnd;
}
