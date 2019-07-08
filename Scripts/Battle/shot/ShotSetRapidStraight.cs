/******************************************************************************/
/*!    \brief  ショットセット：まっすぐ連射 FIXME:シューティング特化なので設計見直しの必要あり.
*******************************************************************************/

using UnityEngine;

namespace VMUnityLib
{
    [System.Serializable]
    public class ShotSetRapidStraight : ShotSet 
    {
	    public int				lineNum = 1;				// ショットライン.
	    public float 			lineWideInterbal = 0.0f;	// ラインの幅.
	    public float			shotSpeed = 0.2f;			// ショットのスピード.
	
	    /// <summary>
	    /// Creates the shot.
	    /// </summary>
	    override public void CreateShot ()
	    {
		    // 指定ライン数生成する.
		    bool isEven = (lineNum % 2 == 0) ? true : false;

		    for(int i=0; i<lineNum; ++i)
		    {
			    Transform newShotTrans = Pool.Spawn(shot.transform);
			    ShotDirectional newShot = newShotTrans.GetComponent<ShotDirectional>();
			    newShot.DamageRate = damageRate;
			    float x;
			    if(isEven)
			    {
				    x = lineWideInterbal * (i - lineNum/2) + lineWideInterbal * 0.5f;
			    }
			    else
			    {
				    x = lineWideInterbal * (i - lineNum/2);
			    }
			    newShot.transform.localRotation = transform.rotation;
			    newShot.transform.position += transform.position + transform.rotation * new Vector3(x, 0, 0);
			    newShot.Pool = Pool;
			    newShot.Velocity = Vector3.forward * shotSpeed;
			    newShot.Accel = 0;
			    if(GetComponent<AudioSource>())
			    {
				    GetComponent<AudioSource>().Play();
			    }
		    }
	    }
    }
}