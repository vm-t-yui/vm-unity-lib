/******************************************************************************/
/*!    \brief  タグネームをエディタ上で選択するためのスクリプト.
*******************************************************************************/

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VMUnityLib
{
    public class TagNameAttribute : PropertyAttribute
    {
        public int selectedValue = 0;
        public bool enableOnly = true;
        public TagNameAttribute(bool enableOnly = true)
        {
            this.enableOnly = enableOnly;
        }
    }


#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(TagNameAttribute))]
    public class TagNameDrawer : PropertyDrawer
    {
        TagNameAttribute tagNameAttribute
        {
            get
            {
                return (TagNameAttribute)attribute;
            }
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string[] tagNames = TagName.GetTagNames();

            if (tagNames.Length == 0)
            {
                EditorGUI.LabelField(position, ObjectNames.NicifyVariableName(property.name), "Tag is Empty");
                return;
            }

            int[] tagNumbers = new int[tagNames.Length];

            SetTagNambers(tagNumbers, tagNames);

            if (!string.IsNullOrEmpty(property.stringValue))
                tagNameAttribute.selectedValue = GetIndex(tagNames, property.stringValue);

            tagNameAttribute.selectedValue = EditorGUI.IntPopup(position, label.text, tagNameAttribute.selectedValue, tagNames, tagNumbers);

            property.stringValue = tagNames[tagNameAttribute.selectedValue];
        }

        void SetTagNambers(int[] tagNumbers, string[] tagNames)
        {
            for (int i = 0; i < tagNames.Length; i++)
            {
                tagNumbers[i] = i;
            }
        }

        int GetIndex(string[] tagNames, string tagName)
        {
            int result = 0;
            for (int i = 0; i < tagNames.Length; i++)
            {
                if (tagName == tagNames[i])
                {
                    result = i;
                    break;
                }
            }
            return result;
        }
    }
#endif
}