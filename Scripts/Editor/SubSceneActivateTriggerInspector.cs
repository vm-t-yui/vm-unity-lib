/******************************************************************************/
/*!    \brief  シーンのルートオブジェクトのエディタ拡張.
*******************************************************************************/

using UnityEngine;
using UnityEditor;

namespace VMUnityLib
{
    [CustomEditor(typeof(SubSceneActivateTrigger))]
    public class SubSceneActivateTriggerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("ゲームオブジェクトの名前を自動設定"))
            {
                CorrectMyName();
            }
        }


        void CorrectMyName()
        {
            SubSceneActivateTrigger obj = target as SubSceneActivateTrigger;
            string newName = obj.gameObject.scene.name + "Trigger";
            if(target.name != newName)
            {
                target.name = newName;
                Undo.RecordObject(obj, "CorrectMyName");
                EditorUtility.SetDirty(target);
            }
        }
    }
}