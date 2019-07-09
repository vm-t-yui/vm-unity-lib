/******************************************************************************/
/*!    \brief  ボイス再生をつかさどる.
*******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

public sealed class VoiceActor : MonoBehaviour 
{
    List<string> reserveActionVoiceIdList;

    /// <summary>
    /// 初期化.
    /// </summary>
    public void Start()
    {
        reserveActionVoiceIdList = new List<string>();
    }

    /// <summary>
    /// アクションボイスの予約セット.
    /// </summary>
    public void ReserveActionVoiceList(List<string> voiceDataIdList)
    {
        reserveActionVoiceIdList.Clear();
        foreach (string str in voiceDataIdList)
        {
            reserveActionVoiceIdList.Add(str);
        }
    }

    /// <summary>
    /// 予約されたアクションボイスを再生する.
    /// </summary>
    void PlayReservedActionVoiceList()
    {
        if (reserveActionVoiceIdList.Count != 0)
        {
            int idx = UnityEngine.Random.Range(0, reserveActionVoiceIdList.Count);
            VoiceManager.Inst.PlayVoice(reserveActionVoiceIdList[idx]);
            reserveActionVoiceIdList.Clear();
        }
    }
}
