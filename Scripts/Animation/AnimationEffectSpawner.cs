/******************************************************************************/
/*!    \brief  アニメーションからエフェクトを発生させる.
*******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using VMUnityLib;

public sealed class AnimationEffectSpawner : MonoBehaviour 
{
    [System.Serializable]
    class EffectSpawnData
    {
        public Transform   baseBone = default;
        public EffectData  effectData = default;
        public Vector3     posOffset = default;
#if UNITY_EDITOR
        public string      comment = default;
#endif
    }
    [SerializeField]
    List<EffectSpawnData> spawnDataList = new List<EffectSpawnData>();

    /// <summary>
    /// アニメーションからエフェクトを生成.
    /// </summary>
    public void CreateEffectByAnimation(int dataIndex)
    {
        Transform baseTrans = transform;
        EffectSpawnData data = spawnDataList[dataIndex];
        if (data.baseBone != null)
        {
            baseTrans = data.baseBone;
        }
        EffectSpawner.Spawn(data.effectData, baseTrans.position + data.posOffset);
    }
}
