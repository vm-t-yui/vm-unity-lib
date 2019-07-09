/******************************************************************************/
/*!    \brief  トリガーのイベントを受け取る.
*******************************************************************************/

using UnityEngine;
namespace VMUnityLib
{
    public class TriggerEventDispatcher : MonoBehaviour
    {
        public delegate void TriggerEventDelegate(Collider collider);

        public GameObject owner;

        public TriggerEventDelegate TriggerEventEnter { get; set; }

        /// <summary>
        /// Use this for initialization
        /// </summary>
        void Start()
        {

        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        void Update()
        {

        }

        /// <summary>
        /// 物理的な当たり判定検知.
        /// </summary>
        void OnTriggerEnter(Collider collider)
        {
            if (TriggerEventEnter != null)
            {
                TriggerEventEnter(collider);
            }
        }
    }
}