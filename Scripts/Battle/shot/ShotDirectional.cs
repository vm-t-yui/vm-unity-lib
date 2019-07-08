/******************************************************************************/
/*!    \brief  方向指定ショット FIXME:シューティング特化なので設計見直しの必要あり.
*******************************************************************************/

using UnityEngine;

namespace VMUnityLib
{
    [System.Serializable]
    public class ShotDirectional : Shot 
    {
#if USE_POOL_MANAGER
        public Vector3        Velocity    { get; set; }    // 速度+方向.
        public float        Accel        { get; set; }    // 加速度.
    
        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        override protected void FixedUpdate () 
        {
            UpdatePosition ();
            base.FixedUpdate ();
        }

        /// <summary>
        /// Updates the position.
        /// </summary>
        protected void UpdatePosition()
        {
            Velocity += Velocity.normalized * Accel;
            transform.localPosition += transform.localRotation * Velocity;
        }
#endif
    }
}
