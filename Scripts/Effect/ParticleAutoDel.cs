using UnityEngine;
#if USE_POOL_MANAGER
using PathologicalGames;
#endif
namespace VMUnityLib
{
    public class ParticleAutoDel : MonoBehaviour
    {
#if USE_POOL_MANAGER
        // a simple script to scale the size, speed and lifetime of a particle system

        public float multiplier = 1;

        void Start()
        {
            var systems = GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem system in systems)
            {
#if !UNITY_5_5_OR_NEWER
                system.startSize *= multiplier;
                system.startSpeed *= multiplier;
                system.startLifetime *= Mathf.Lerp(multiplier, 1, 0.5f);
#endif
                system.Clear();
                system.Play();
            }
        }

        // Update is called once per frame
        void Update()
        {
            var systems = GetComponentsInChildren<ParticleSystem>();
            bool timeToDie = true;
            foreach (ParticleSystem system in systems)
            {
                if (system.isStopped == false)
                {
                    timeToDie = false;
                    break;
                }
            }
            if (timeToDie)
            {
                PoolManager.Pools[PoolName.Effect].Despawn(transform);
            }
        }
#endif
    }
}