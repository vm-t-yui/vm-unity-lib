/******************************************************************************/
/*!    \brief  アニメーションからマテリアルチェンジを行う.
*******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

public sealed class AnimationMaterialChanger : MonoBehaviour
{
    [System.Serializable]
    struct ChangeMaterialData
    {
        public Material mat;
#if UNITY_EDITOR
        public string comment;
#endif
    }
    [SerializeField]
    List<ChangeMaterialData> matChangeDataList;

    [SerializeField]
    new Renderer renderer;

    /// <summary>
    /// アニメーションからマテリアルチェンジ.
    /// </summary>
    public void ChangeMaterialByAnimation(int dataIndex)
    {
        ChangeMaterialData data = matChangeDataList[dataIndex];
        renderer.material = data.mat;
    }
}
