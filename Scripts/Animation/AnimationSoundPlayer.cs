/******************************************************************************/
/*!    \brief  アニメーションからサウンドを再生させる.
*******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using VMUnityLib;

public sealed class AnimationSoundPlayer : MonoBehaviour 
{
    [SerializeField]
    RandomSoundSelecter soundSelecter = default;

    public RandomSoundSelecter SoundSelecter { get { return soundSelecter; } }

    [System.Serializable]
    class SoundData
    {
        public List<AudioClip>      clip = default;
        public float                pitchBand = default;
        public float                playPercent = default;
#if UNITY_EDITOR
        public string comment = default;
#endif
    }

    [SerializeField]
    List<SoundData> dataList = default;

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
