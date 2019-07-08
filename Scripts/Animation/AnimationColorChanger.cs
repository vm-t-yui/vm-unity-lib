/******************************************************************************/
/*!    \brief  アニメーションからカラーチェンジを行う.
*******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VMUnityLib
{
    public sealed class AnimationColorChanger : MonoBehaviour
    {
#if USE_TWEEN
        [System.Serializable]
        struct ChangeColorData
        {
            public Color color;
            public float time;
            public iTween.EaseType easeType;
            public List<Renderer> etcTargets; // 追加で同じ効果を発生させるレンダラー.

#if UNITY_EDITOR
            public string comment;
#endif
        }
        [SerializeField]
        List<ChangeColorData> colorChangeDataList;

        /// <summary>
        /// アニメーションからカラーチェンジ.
        /// </summary>
        public void ChangeColorByAnimation(int dataIndex)
        {
            ChangeColorData data = colorChangeDataList[dataIndex];
            Hashtable table = new Hashtable();
            table.Add("color", data.color);
            table.Add("time", data.time);
            table.Add("easetype", data.easeType);
            //table.Add("a", data.color.a);
            iTween.ColorTo(gameObject, table);
            foreach (var item in data.etcTargets)
            {
                Hashtable table2 = new Hashtable();
                table2.Add("color", data.color);
                table2.Add("time", data.time);
                table2.Add("easetype", data.easeType);
                //table.Add("a", data.color.a);
                iTween.ColorTo(item.gameObject, table);
            }
        }
#endif
    }
}
