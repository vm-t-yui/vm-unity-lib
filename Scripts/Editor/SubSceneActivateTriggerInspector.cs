using UnityEngine;
using UnityEditor;

namespace VMUnityLib
{
    /// <summary>
    /// サブシーンルートトリガー
    /// </summary>
    [CustomEditor(typeof(SubSceneActivateTrigger))]
    public class SubSceneActivateTriggerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.HelpBox("隣のサブシーンとの切り替え用のトリガーは重なっている必要があり、これから読み込む側のトリガーが先にヒットする必要がある", MessageType.Info);
            if (GUILayout.Button("ゲームオブジェクトの名前を自動設定"))
            {
                CorrectMyName();
            }
        }

        /// <summary>
        /// 名前自動設定
        /// </summary>
        void CorrectMyName()
        {
            SubSceneActivateTrigger obj = target as SubSceneActivateTrigger;
            string newName = obj.gameObject.scene.name + "Trigger";
            if(target.name != newName && !obj.IsOnRoot)
            {
                obj.DebugSetTargetSceneName(obj.gameObject.scene.name);
                target.name = newName;
                Undo.RecordObject(obj, "CorrectMyName");
                EditorUtility.SetDirty(target);
            }
        }
    }
}