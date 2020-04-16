/******************************************************************************/
/*!    \brief  シーンのルートオブジェクトのエディタ拡張.
*******************************************************************************/

using UnityEditor;

namespace VMUnityLib
{
    [CustomEditor(typeof(SubSceneRoot))]
    public class SubSceneRootInspector : Editor
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
            SubSceneRoot obj = target as SubSceneRoot;
            string newName = SceneManager.SUBSCENE_ROOT_NAME_HEADER + obj.gameObject.scene.name;
            if(target.name != newName)
            {
                target.name = newName;
                Undo.RecordObject(obj, "CorrectMyName");
                EditorUtility.SetDirty(target);
            }
        }
    }
}