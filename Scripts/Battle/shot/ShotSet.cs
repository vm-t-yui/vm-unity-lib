/******************************************************************************/
/*!    \brief  複数のショットをグループで起動するセットの基底 FIXME:シューティング特化なので設計見直しの必要あり.
*******************************************************************************/

using UnityEngine;
using PathologicalGames;

namespace VMUnityLib
{
    public delegate void OnShotEvent();

    public abstract class ShotSet : MonoBehaviour 
    {
	    public Shot			shot;				// ショット;
	    public float 		shotInterval;		// ショット間隔.
	    public float		damageRate = 1.0f;	// ショットのダメージ倍率.

	    public bool			isShotCreateDirect = true; // 直接ショットを生成するか.
	    public OnShotEvent	onShotEvent;

	    private float		prevShotTime;		// 直前に撃ったショット時間.

	    public		bool		IsShotNow	{ get; private set; }
	    protected	SpawnPool	Pool 		{ get; private set; }
	
        /// <summary>
        /// プールを設定.
        /// </summary>
        public void SetPool(SpawnPool pool)
	    {
		    Pool = pool;
	    }
	
	    /// <summary>
	    /// Update is called once per frame.
	    /// </summary>
	    virtual protected void Update () 
	    {
		    if(IsShotNow)
		    {
			    // 決められた間隔でショット生成.
			    float nowTime = Time.time;
			    if(nowTime - prevShotTime > shotInterval)
			    {
				    prevShotTime = nowTime;
				    CreateShotEvent();
			    }
		    }
	    }

	    /// <summary>
	    /// ショットタイムのカウントをリセットする.
	    /// </summary>
	    public void ResetCount()
	    {
		    prevShotTime = Time.time;
	    }
	
	    /// <summary>
	    /// ショット生成イベント（直接生成せず、モーションで生成する場合があるため）.
	    /// </summary>
	    private void CreateShotEvent ()
	    {
		    if(isShotCreateDirect)
		    {
			    CreateShot ();
		    }
		    if(onShotEvent != null)
		    {
			    onShotEvent();
		    }
	    }

	    /// <summary>
	    /// Creates the shot.
	    /// </summary>
	    abstract public void CreateShot ();

	    /// <summary>
	    /// Starts the shot.
	    /// </summary>
	    virtual public void StartShot()
	    {
		    IsShotNow = true;
		    prevShotTime = Time.time;
	    }

	    /// <summary>
	    /// Stops the shot.
	    /// </summary>
	    virtual public void StopShot()
	    {
		    IsShotNow = false;
	    }
    }
}
