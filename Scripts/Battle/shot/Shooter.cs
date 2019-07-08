/******************************************************************************/
/*!    \brief  指定インターバルでショットセットを起動させる発射装置 FIXME:シューティング特化なので設計見直しの必要あり.
*******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace VMUnityLib
{
    public class Shooter : MonoBehaviour 
    {
#if USE_POOL_MANAGER
        public List<ShotSet>    shotSetPrefabs;
        public bool                shootOnStart = false;

        public bool IsShotStarted { get; private set; }

        public List<ShotSet>    ShotSetList { get; private set; }
    
        /// <summary>
        /// Use this for initialization.
        /// </summary>
        void Start () 
        {
            // ショットセットを生成.
            ShotSetList = new List<ShotSet> ();
            foreach(ShotSet setPrefab in shotSetPrefabs)
            {
                GameObject    newShotSetObj = (GameObject)GameObject.Instantiate(setPrefab.gameObject);
                ShotSet        newShotSet = newShotSetObj.GetComponent<ShotSet>();
                newShotSet.transform.parent = transform;
                newShotSet.transform.localPosition = Vector3.zero;
                newShotSet.transform.localRotation = Quaternion.identity;
                ShotSetList.Add(newShotSet);
            }
            if(shootOnStart == true)
            {
                StartShot();
            }
        }
    
        /// <summary>
        /// シーン切り替え時の初期化.
        /// </summary>
        void InitSceneChange()
        {
            Start();
        }
    
        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        void Update () 
        {
        }

        /// <summary>
        /// Starts the shot.
        /// </summary>
        public void StartShot()
        {
            IsShotStarted = true;
            foreach(ShotSet set in ShotSetList)
            {
                set.StartShot();
            }
        }

        /// <summary>
        /// Stops the shot.
        /// </summary>
        public void StopShot()
        {
            if (ShotSetList == null) return;
            IsShotStarted = false;
            foreach(ShotSet set in ShotSetList)
            {
                set.StopShot();
            }
        }
#endif
    }
}