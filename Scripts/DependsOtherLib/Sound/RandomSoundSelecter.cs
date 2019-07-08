/******************************************************************************/
/*!    \brief  音のランダム再生.
*******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
namespace VMUnityLib
{
    public sealed class RandomSoundSelecter : MonoBehaviour
    {
        [SerializeField] bool bAutoPlay = false;                             // Start時に自動再生するかどうか.
        [SerializeField] List<AudioClip> audioList = new List<AudioClip>();  // オーディオリスト.
        [SerializeField] float randomPitchBand = 0.0f;                       // 再生時のランダムピッチ幅.
        [SerializeField] float playPercent = 100.0f;                         // 再生する確率.
        [SerializeField] float overrideEnableTime = 0.05f;                   // 再生上書きが有効になる経過時間.

        private AudioSource audioSource;
        private float defaultPitch;

        float lastPlayTime = 0;

        /// <summary>
        /// 初期化.
        /// </summary>
        public void Start()
        {
            if (audioSource == null)
            {
                InitAudioSource();
            }
        }

        /// <summary>
        /// 有効化時.
        /// </summary>
        public void OnEnable()
        {
            if (audioSource == null)
            {
                InitAudioSource();
            }
            if (bAutoPlay && audioSource != null)
            {
                Play();
            }
        }

        /// <summary>
        /// ランダム再生.
        /// </summary>
        public void Play()
        {
            bool bPlay = (Random.Range(0, 100.0f) < playPercent) ? true : false;
            if (bPlay)
            {
                int idx = Random.Range(0, audioList.Count);

                // 既にプレイ中だった場合、上書き可能時間が過ぎていなければプレイしない.
                if (audioSource.isPlaying)
                {
                    if(Time.time - lastPlayTime < overrideEnableTime)
                    {
                        bPlay = false;
                    }
                }
                if(bPlay)
                {
                    audioSource.clip = audioList[idx];
                    audioSource.pitch = defaultPitch + Random.Range(-randomPitchBand, randomPitchBand);
                    audioSource.Play();
                    lastPlayTime = Time.time;
                }
            }
        }

        /// <summary>
        /// ランダムピッチ幅設定.
        /// </summary>
        public void SetPitcchBand(float band)
        {
            randomPitchBand = band;
        }

        /// <summary>
        /// オーディオリスト更新.
        /// </summary>
        public void SetAudioList(List<AudioClip> set)
        {
            audioList = set;
        }

        /// <summary>
        /// プレイ率設定.
        /// </summary>
        public void SetPlayPercent(float set)
        {
            playPercent = set;
        }

        /// <summary>
        /// オーディオソース初期化.
        /// </summary>
        public void InitAudioSource()
        {
            audioSource = GetComponent<AudioSource>();
            defaultPitch = audioSource.pitch;
        }
    }
}