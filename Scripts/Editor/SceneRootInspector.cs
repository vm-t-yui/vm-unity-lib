/******************************************************************************/
/*!    \brief  シーンのルートオブジェクトのエディタ拡張.
*******************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace VMUnityLib
{
    [CustomEditor(typeof(SceneRoot))]
    public class SceneRootInspector : Editor
    {
        public void OnEnable()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                CorrectMyName();
            }
        }

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
            SceneRoot obj = target as SceneRoot;
            //string sceneName = EditorApplication.currentScene;
            string sceneName = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name;
            //while (true)
            //{
            //    int index = sceneName.IndexOf("/");
            //    if (index >= 0)
            //    {
            //        sceneName = sceneName.Remove(0, index + 1);
            //    }
            //    else
            //    {
            //        break;
            //    }
            //}
            //sceneName = sceneName.Substring(0, sceneName.LastIndexOf("."));
            target.name = SceneManager.SCENE_ROOT_NAME_HEADER + sceneName;
            Undo.RecordObject(obj, "CorrectMyName");
            EditorUtility.SetDirty(target);
        }
    }
}