/******************************************************************************/
/*!    \brief  アニメーションからサウンドを再生させる.
*******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using VMUnityLib;

public sealed class AnimationSoundPlayer : MonoBehaviour 
{
    [SerializeField]
    private RandomSoundSelecter soundSelecter;

    public RandomSoundSelecter SoundSelecter { get { return soundSelecter; } }

    [System.Serializable]
    struct SoundData
    {
        public List<AudioClip>      clip;
        public float                pitchBand;
        public float                playPercent;
#if UNITY_EDITOR
        public string comment;
#endif
    }

    [SerializeField]
    List<SoundData> dataList;

    /// <summary>
    /// アニメーションからサウンドを再生.
    /// </summary>
    public void PlaySoundByAnimation(int dataIndex)
    {
        SoundData data = dataList[dataIndex];
        soundSelecter.SetAudioList(data.clip);
        soundSelecter.SetPitcchBand(data.pitchBand);
        soundSelecter.SetPlayPercent(data.playPercent);
        soundSelecter.Play();
    }
}
