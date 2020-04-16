/******************************************************************************/
/*!    \brief  シーンのルートオブジェクトのエディタ拡張.
*******************************************************************************/

using UnityEditor;

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

        void CorrectMyName()
        {
            SceneRoot obj = target as SceneRoot;
            string newName = SceneManager.SCENE_ROOT_NAME_HEADER + obj.gameObject.scene.name;
            if (target.name != newName)
            {
                target.name = newName;
                Undo.RecordObject(obj, "CorrectMyName");
                EditorUtility.SetDirty(target);
            }
        }
    }
}