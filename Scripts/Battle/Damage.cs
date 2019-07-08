/******************************************************************************/
/*!    \brief  ダメージ.
*******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace VMUnityLib
{
    /// <summary>
    /// Damage result.
    /// </summary>
    public struct DamageResult
    {
        public DamageInfo   TookDamage  { get; private set; }
        public GameObject   Target      { get; private set; }
        public DamageResult(DamageInfo tookDamage, GameObject target)
            : this()
        {
            TookDamage = tookDamage;
            Target = target;
        }
    }

    public enum DamageKind
    {
        SINGLE,         // 単発ダメージ.
        SLIP            // スリップダメージ.
    }

    public enum DamageAttribute
    {
        PENETRATION,    // 貫通.
        DONT_DEATH      // 死なない.
    }

    [System.Serializable]
    public class Damage
    {
        public const string OnTakeDamageEventName    = "OnTakeDamage";
        public const string OnProvideDamageEventName = "OnProvideDamage";

        [SerializeField] private DamageKind             kind;           // ダメージ種別.
        [SerializeField] private List<DamageAttribute>  attribute;      // ダメージ属性.
        [SerializeField] private int                    pureDamage;     // ダメージ量.
        [SerializeField] private string                 damageEffectId; // ダメージエフェクトId.
        [SerializeField] private DamageEtcEffect        etcEffect;      // ゲームごとの特殊エフェクト.

        public GameObject       Owner       { get; set; }               // 持ち主.
        public float            DamageRate  { get; set; }

        public DamageKind            Kind           { get { return kind;            } }
        public List<DamageAttribute> Attribute      { get { return attribute;       } }
        public int                   PureDamage     { get { return pureDamage;      } }
        public string                DamageEffectId { get { return damageEffectId;  } }

        public DamageEtcEffect EtcEffect { get { return etcEffect; } set { etcEffect = value; } }

        /// <summary>
        /// コンストラクタ.
        /// </summary>
        public Damage(
                DamageKind              inKind,
                List<DamageAttribute>   inAttribute,
                int                     inPureDamage,
                DamageEtcEffect         inEtcEffect,
                string                  inDamageEffectId,
                float                   inDamageRate,
                GameObject              inOwner
            )
        {
            kind = inKind;
            attribute = new List<DamageAttribute>();
            foreach (DamageAttribute attr in inAttribute)
            {
                attribute.Add(attr);
            }
            pureDamage = inPureDamage;
            damageEffectId = inDamageEffectId;
            DamageRate = inDamageRate;
            Owner = inOwner;
            etcEffect = inEtcEffect;
        }

        /// <summary>
        /// コンストラクタ.
        /// </summary>
        public Damage(Damage src)
        {
            kind = src.kind;
            attribute = new List<DamageAttribute>();
            foreach (DamageAttribute attr in src.attribute)
            {
                attribute.Add(attr);
            }
            pureDamage = src.pureDamage;
            damageEffectId = src.damageEffectId;
            DamageRate = src.DamageRate;
            Owner = src.Owner;
            EtcEffect = src.etcEffect;
        }

        /// <summary>
        /// ダメージレート計算済みの値を返す.
        /// </summary>
        public int GetDamageNum() 
        { 
            return (int)((float)pureDamage * DamageRate); 
        }

        /// <summary>
        /// 指定アトリビュートを持っているかどうか.
        /// </summary>
        public bool HasAttr(DamageAttribute inAttr)
        {
            bool ret = false;
            foreach (DamageAttribute attr in attribute)
            {
                if(attr == inAttr)
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }
    }
}
