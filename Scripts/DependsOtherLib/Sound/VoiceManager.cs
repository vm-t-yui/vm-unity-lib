/******************************************************************************/
/*!    \brief  ボイスを再生するだけ。
*******************************************************************************/

using UnityEngine;
using System;

namespace VMUnityLib
{
    public class VoiceManager : SingletonMonoBehaviour<VoiceManager>
    {
        [SerializeField]
        SePlayer sePlayer = default;

        /// <summary>
        /// ボイス再生.
        /// </summary>
        public void PlayVoice(string id, Action onEndPlayVoice = null)
        {
            VoiceData voiceData;
            GameDataManager.Inst.VoiceDataManager.GetData(id, out voiceData);
            if (voiceData)
            {
                // 字幕
                if (voiceData.TermName != string.Empty)
                {
                    UISubtitle.Inst.SetSubtiltle(voiceData.TermName, voiceData.Time, onEndPlayVoice);
                }
                // 再生
                if (voiceData.TermName != string.Empty)
                {
                    sePlayer.PlaySe(voiceData.TermName);
                }
            }
        }
    }
}