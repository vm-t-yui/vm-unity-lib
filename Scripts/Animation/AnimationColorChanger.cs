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
        class ChangeColorData
        {
            public Color color = default;
            public float time = default;
            public iTween.EaseType easeType = default;
            public List<Renderer> etcTargets = default; // 追加で同じ効果を発生させるレンダラー.

#if UNITY_EDITOR
            public string comment = default;
#endif
        }
        [SerializeField]
        List<ChangeColorData> colorChangeDataList = default;

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
