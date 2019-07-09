/******************************************************************************/
/*!    \brief  アニメーションからマテリアルチェンジを行う.
*******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

public sealed class AnimationMaterialChanger : MonoBehaviour
{
    [System.Serializable]
    class ChangeMaterialData
    {
        public Material mat = default;
#if UNITY_EDITOR
        public string comment = default;
#endif
    }
    [SerializeField]
    List<ChangeMaterialData> matChangeDataList = default;

    [SerializeField]
    new Renderer renderer = default;

    /// <summary>
    /// アニメーションからマテリアルチェンジ.
    /// </summary>
    public void ChangeMaterialByAnimation(int dataIndex)
    {
        ChangeMaterialData data = matChangeDataList[dataIndex];
        renderer.material = data.mat;
    }
}
