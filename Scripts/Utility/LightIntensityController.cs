/******************************************************************************/
/*!    \brief  ライトの強さを管理するユーティリティ.
*******************************************************************************/

using UnityEngine;

public sealed class LightIntensityController : MonoBehaviour 
{
    [SerializeField]
    private float intensityMax;

    [SerializeField]
    private float intensityMin;

    [SerializeField]
    private Light targetLight;

    [SerializeField]
    private bool FadeOutOnActive = false;

    [SerializeField]
    private float FadeOutLerpT;

    private bool isFading = false;
    private float random;

    /// <summary>
    /// アクティブ時.
    /// </summary>
    private void OnEnable()
    {
        if(FadeOutOnActive)
        {
            FadeOutLerp(FadeOutLerpT);
        }
    }

    /// <summary>
    /// 更新.
    /// </summary>
    private void FixedUpdate()
    {
        if(isFading == false)
        {
            random = Random.Range(0.0f, 150.0f);
            float noise = Mathf.PerlinNoise(random, Time.time);
            targetLight.intensity = Mathf.Lerp(intensityMin, intensityMax, noise);
        }
    }

    /// <summary>
    /// 線形に消えていく。最後は自動でgameObjectをDeactiveに.
    /// </summary>
    public void FadeOutLerp(float t)
    {
        isFading = true;
        StopCoroutine("FadeOutCoroutine");
        StartCoroutine(FadeOutCoroutine(t));
    }

    System.Collections.IEnumerator FadeOutCoroutine(float t)
    {
        while (targetLight.intensity > 0.001f)
        {
            targetLight.intensity = Mathf.Lerp(targetLight.intensity, 0, t);
            yield return LibBridgeInfo.WaitForEndOfFrame;
        }
        gameObject.SetActive(false);
        isFading = false;
    }
}
