/******************************************************************************/
/*!    \brief  ダメージの種別と威力を持って対称にダメージを発行する.
*******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace VMUnityLib
{
    public class DamageField : MonoBehaviour 
    {
        public enum ContactPointKind
        {
            CONTACT_POINT,      // 接触地点.
            OWNER_POINT,        // オーナーオブジェクトの中心.
            TARGET_POINT        // 相手オブジェクトの中心.
        }

        class SlippingObjectData
        {
            public GameObject           target;              // スリップダメージ対象.
            public float                prevTime;            // 最後にスリップダメージが通った時間.
            public SlippingObjectData(GameObject inTarget, float inPrevTime)
            {
                target = inTarget;
                prevTime = inPrevTime;
            }
        }

        [SerializeField] float                slipInterbal      = default;              // スリップダメージ間隔（秒）.
        [SerializeField] Damage               damage            = default;              // 基本ダメージ.
        [SerializeField] ContactPointKind     contactPointKind  = default;              // 通知する接触ポイント.
        [SerializeField] GameObject           owner             = default;              // オーナー.

        [TagNameAttribute, SerializeField] 
        public List<string>                           targetTags;                               // ダメージ対象タグ.

        public GameObject                             Target { get; set; }                      // ターゲット。特定の対象を指定する場合に使用。PENETRATIONより優先度高.
        public Damage                                 Damage { get { return damage; } }

        List<SlippingObjectData>    slippingObjects = new List<SlippingObjectData>();   // スリップダメージを通しているオブジェクトの情報リスト.
    
        /// <summary>
        /// Use this for initialization.
        /// </summary>
        void Start () 
        {
            // NULLに設定されている場合は、外部から設定する.
            if (owner != null)
            {
                damage.Owner = owner;
            }
            damage.DamageRate = 1.0f;
        }

        /// <summary>
        /// 外部生成用の初期化.
        /// </summary>
        public void InitCreateOutside(Damage newDamage)
        {
            // 外部生成なのでOwnerを上書き.
            owner = newDamage.Owner;
            damage = newDamage;
        }

        /// <summary>
        /// 当たり判定検知.
        /// </summary>
        void OnTriggerEnter(Collider collider)
        {
            if(damage.Owner.activeSelf == false) return;
            if(gameObject.activeSelf == false) return;

            GameObject target = CheckIsTargetObject(collider.gameObject);
            if (target && target.activeSelf)
            {
                switch (damage.Kind)
                {
                case DamageKind.SLIP:
                    SlippingObjectData data = new SlippingObjectData(target, Time.timeScale);
                    slippingObjects.Add(data);
                    break;

                case DamageKind.SINGLE:
                    DamageSender.TakeDamage(target, damage, GetContactPos(target));
                    // 貫通でないなら一度与えた時点で非表示にする.
                    if (damage.HasAttr(DamageAttribute.PENETRATION) == false)
                    {
                        gameObject.SetActive(false);
                    }
                    break;
                default:
                    break;
                }
            }
        }

        /// <summary>
        /// Raises the trigger stay event.
        /// </summary>
        void OnTriggerStay(Collider collider)
        {
            if(damage.Owner.activeSelf == false) return;
            if(gameObject.activeSelf == false) return;
        
            GameObject obj = CheckIsTargetObject(collider.gameObject);
            if(obj && obj.activeSelf)
            {
                switch(damage.Kind)
                {
                    case DamageKind.SLIP:
                        TakeSlipDamage(collider.gameObject, GetContactPos(collider.gameObject));
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Raises the trigger exit event.
        /// </summary>
        /// <param name="other">Other.</param>
        void OnTriggerExit(Collider other)
        {
            GameObject obj = CheckIsTargetObject((GetComponent<Collider>().gameObject));
            if(obj)
            {
                slippingObjects.RemoveAll(d => d.target == obj);
            }
        }

        /// <summary>
        /// スリップダメージを与える.
        /// </summary>
        void TakeSlipDamage(GameObject target, Vector3 effectPos)
        {
            foreach (SlippingObjectData objdata in slippingObjects)
            {
                if (objdata.target == target)
                {
                    if (Time.time - objdata.prevTime > slipInterbal)
                    {
                        DamageSender.TakeDamage(target, damage, GetContactPos(target));
                        objdata.prevTime = Time.time;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// ターゲットかどうか調べる.
        /// </summary>
        /// <returns>The target objects.</returns>
        GameObject CheckIsTargetObject(GameObject target)
        {
            GameObject ret = null;
            if (Target == null)
            {
                foreach (string tag in targetTags)
                {
                    if (target.CompareTag(tag))
                    {
                        ret = target;
                        break;
                    }
                }
            }
            // 指定ターゲットがいる場合はそいつかどうかだけチェック.
            else
            {
                if (Target == target)
                {
                    ret = target;
                }
            }
            return ret;
        }

        /// <summary>
        /// 接触ポジションを種別ごとに取得する.
        /// </summary>
        Vector3 GetContactPos(GameObject target)
        {
            Vector3 ret = Vector3.zero;
            switch(contactPointKind)
            {
                case ContactPointKind.CONTACT_POINT:
                    // 反対方向に離れた箇所からレイを飛ばして、ターゲットと接触した地点が接触点.
                    // HACK:コライダとゲームオブジェクトの位置が著しく離れていると無意味.
                    Vector3 targetToOwner  =  damage.Owner.transform.position - target.transform.position;
                    Vector3 targetToOwnerN = targetToOwner;
                    targetToOwnerN.Normalize();
                    Vector3 rayStartPos = damage.Owner.transform.position + targetToOwnerN * 500.0f;    // アバウトにとりあえず500としている.
                
                    // Ray を飛ばす.
                    RaycastHit[] hits = Physics.RaycastAll(rayStartPos, -targetToOwnerN, 1000.0f);
                    bool bhit = false;
                    foreach (RaycastHit hit in hits)
                    {
                        if(hit.transform.gameObject == target)
                        {
                            ret = hit.point;
                            bhit = true;
                            break;
                        }
                    }
                    if(bhit == false)
                    {
                        //Logger.Warn("RaycastHit Failed.");
                        ret = target.transform.position;
                    }
                    break;
                case ContactPointKind.OWNER_POINT:
                    ret = damage.Owner.transform.position;
                    break;
                case ContactPointKind.TARGET_POINT:
                    ret = target.transform.position;
                    break;
                default:
                    break;
            }
            return ret;
        }
    }
}