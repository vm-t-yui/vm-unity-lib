/******************************************************************************/
/*!    \brief  アニメーションからエフェクトを発生させる.
*******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using VMUnityLib;

public sealed class AnimationEffectSpawner : MonoBehaviour 
{
    [System.Serializable]
    struct EffectSpawnData
    {
        public Transform   baseBone;
        public EffectData  effectData;
        public Vector3     posOffset;
#if UNITY_EDITOR
        public string      comment;
#endif
    }
    [SerializeField]
    List<EffectSpawnData> spawnDataList;

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
