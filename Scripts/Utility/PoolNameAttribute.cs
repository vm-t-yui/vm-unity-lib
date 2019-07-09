/******************************************************************************/
/*!    \brief  タグネームをエディタ上で選択するためのスクリプト.
*******************************************************************************/

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VMUnityLib
{

    public class PoolNameAttribute : PropertyAttribute
    {
        public int selectedValue = 0;
        public bool enableOnly = true;
        public PoolNameAttribute(bool enableOnly = true)
        {
            this.enableOnly = enableOnly;
        }
    }


#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(PoolNameAttribute))]
    public class PoolNameDrawer : PropertyDrawer
    {
        PoolNameAttribute poolNameAttribute
        {
            get
            {
                return (PoolNameAttribute)attribute;
            }
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string[] poolNames = PoolName.GetPoolNames();

            if (poolNames.Length == 0)
            {
                EditorGUI.LabelField(position, ObjectNames.NicifyVariableName(property.name), "Pool is Empty");
                return;
            }

            int[] poolNumbers = new int[poolNames.Length];

            SetPoolNambers(poolNumbers, poolNames);

            if (!string.IsNullOrEmpty(property.stringValue))
                poolNameAttribute.selectedValue = GetIndex(poolNames, property.stringValue);

            poolNameAttribute.selectedValue = EditorGUI.IntPopup(position, label.text, poolNameAttribute.selectedValue, poolNames, poolNumbers);

            property.stringValue = poolNames[poolNameAttribute.selectedValue];
        }

        void SetPoolNambers(int[] poolNumbers, string[] poolNames)
        {
            for (int i = 0; i < poolNames.Length; i++)
            {
                poolNumbers[i] = i;
            }
        }

        int GetIndex(string[] poolNames, string poolName)
        {
            int result = 0;
            for (int i = 0; i < poolNames.Length; i++)
            {
                if (poolName == poolNames[i])
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