/******************************************************************************/
/*!    \brief  エフェクト生成を行う.
*******************************************************************************/

using UnityEngine;
#if USE_POOL_MANAGER
using PathologicalGames;
#endif

namespace VMUnityLib
{
    public sealed class EffectSpawner
    {
        /// <summary>
        /// エフェクト生成.
        /// </summary>
        /// <returns>生成したエフェクト</returns>
        public static Transform Spawn(string id, Vector3 pos, Quaternion rot)
        {
            EffectData effectData;
            GameDataManager.Inst.EffectDataManager.GetData(id, out effectData);
            return Spawn(effectData, pos, rot);
        }
        public static Transform Spawn(string id, Vector3 pos)
        {
            EffectData effectData;
            GameDataManager.Inst.EffectDataManager.GetData(id, out effectData);
            return Spawn(effectData, pos, effectData.Prefab.transform.rotation);
        }
        public static Transform Spawn(EffectData effectData, Vector3 pos, Quaternion rot)
        {
#if USE_POOL_MANAGER
            SpawnPool pool = PoolManager.Pools[PoolName.Effect];
            Transform trans = pool.Spawn(effectData.Prefab.transform, pos, rot);
            return trans;
#else
            Debug.LogError("you need include Pool Manager.");
            return null;
#endif
        }
        public static Transform Spawn(EffectData effectData, Vector3 pos)
        {
#if USE_POOL_MANAGER
            SpawnPool pool = PoolManager.Pools[PoolName.Effect];
            Transform trans = pool.Spawn(effectData.Prefab.transform, pos, effectData.Prefab.transform.rotation);
            return trans;
#else
            Debug.LogError("you need include Pool Manager.");
            return null;
#endif
        }
    }
}
