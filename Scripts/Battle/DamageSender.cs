/******************************************************************************/
/*!    \brief  ダメージを発行する.
*******************************************************************************/

using UnityEngine;

namespace VMUnityLib
{
    /// <summary>
    /// Damage result.
    /// </summary>
    public struct DamageInfo
    {
        public Damage       Damage      { get; private set; }
        public Vector3      ContactPos  { get; private set; }
        public DamageInfo(Damage damage, Vector3 contactPos)
            : this()
        {
            Damage = damage;
            ContactPos = contactPos;
        }
    }

    public class DamageSender
    {
        /// <summary>
        /// Takes the single damage.
        /// </summary>
        public static void TakeDamage(GameObject target, Damage damage, Vector3 contactPos)
        {
            Damage damageClone = new Damage(damage);

            // ダメージを送る.
		    if(damageClone.Owner != target && target.isStatic == false && target.activeSelf == true)
		    {
                DamageInfo damageInfo = new DamageInfo(damageClone, contactPos);
                target.SendMessage(Damage.OnTakeDamageEventName, damageInfo);
                DamageResult res = new DamageResult(damageInfo, target);
			    if (damageClone.Owner != null)
	            {
	                damageClone.Owner.SendMessage(Damage.OnProvideDamageEventName, res);
	            }
		    }
        }
    }
}