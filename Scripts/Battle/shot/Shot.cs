/******************************************************************************/
/*!    \brief  ショットの最小単位.
*******************************************************************************/

using UnityEngine;
#if USE_POOL_MANAGER
using PathologicalGames;
#endif

namespace VMUnityLib
{
    public abstract class Shot : MonoBehaviour 
    {
#if USE_POOL_MANAGER
        public int            hp;                // 耐久力.
        public float        lifeTime;        // 寿命.

        private float         startTime;

        public    SpawnPool    Pool         { get; set; }
        public    float        DamageRate     { get; set; }
        public    DamageField DamageField { get; private set; }

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        virtual public void OnSpawned() 
        {
            startTime = Time.time;
            DamageField = transform.GetComponent<DamageField>();
            if (DamageField == null)
            {
                DamageField = transform.GetComponentInChildren<DamageField>();
            }
            Damage damage = DamageField.Damage;
            damage.DamageRate = DamageRate;

        }
    
        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        virtual protected void FixedUpdate () 
        {
            if(Pool && Time.time - startTime > lifeTime)
            {
                Pool.Despawn(gameObject.transform);
            }
        }

        /// <summary>
        /// Raises the take damage event.
        /// </summary>
        /// <param name="damage">Damage.</param>
        void OnTakeDamage(Damage damage)
        {
            //Logger.Log ("kind:" + damage.kind +" attr:" + damage.attribute + " num:" + damage.damageNum);
        }

        /// <summary>
        /// Raises the provide damage event.
        /// </summary>
        /// <param name="result">Result.</param>
        void OnProvideDamage(DamageResult result)
        {
            //Logger.Log ("kind:" + result.TookDamage.kind +" attr:" + result.TookDamage.attribute + " num:" + result.TookDamage.damageNum);
            if(Pool && DamageField.Damage.HasAttr(DamageAttribute.PENETRATION) == false)
            {
                Pool.Despawn(gameObject.transform);
            }
        }
#endif
    }
}