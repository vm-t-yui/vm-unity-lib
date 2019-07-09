/******************************************************************************/
/*!    \brief  Tweenをトグルする(ボタン無効化機能付き).
*******************************************************************************/

using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using VMUnityLib;
using UnityEngine.UI;

namespace VMUnityLib
{
    /// <summary>
    /// masterTweenKindを設定し、その種別と同じtweenを設定しなければならない.
    /// </summary>
    public class ToggleTween : MonoBehaviour
    {
#if USE_TWEEN
        [System.Serializable]
        public enum MasterTweenKind
        {
            ALPHA,
            SCALE,
            POS,
            ROT,
            PATH
        }

        enum AutoCloseState
        {
            NONE,
            CLOSE_REQUESTED,
            CLOSING
        }

        [SerializeField]
        private MasterTweenKind masterTweenKind;    // Tweenが終了したことを判定するためのtween種別.

        [SerializeField]
        private bool autoClose;          // 自動で閉じるかどうか.

        [SerializeField]
        private float autoCloseTime;      // 自動でとじるのであれば閉じるまでの時間.

        [SerializeField]
        private UnityEvent onFinishAutoClose;   // 自動で閉じるイベントが終了したとき.

        uTweener masterTween;        // Tweenが終了したことを判定するためのtween.
        List<uTweener> tweens;
        float tweenStartTime;
        AutoCloseState autoCloseState;

        /// <summary>
        /// 生成時.
        /// </summary>
        void Awake()
        {
            tweens = new List<uTweener>();
            foreach (uTweener tween in GetComponents<uTweener>())
            {
                if (masterTween == null)
                {
                    switch (masterTweenKind)
                    {
                        case MasterTweenKind.ALPHA:
                            masterTween = (uTweenAlpha)tween;
                            break;
                        case MasterTweenKind.SCALE:
                            masterTween = (uTweenScale)tween;
                            break;
                        case MasterTweenKind.POS:
                            masterTween = (uTweenPosition)tween;
                            break;
                        case MasterTweenKind.ROT:
                            masterTween = (uTweenRotation)tween;
                            break;
                        case MasterTweenKind.PATH:
                            masterTween = (uTweenPath)tween;
                            break;
                    }
                }
                tweens.Add(tween);
            }
            if (masterTween == null)
            {
                Debug.LogError("plaese add selected kind of tween! :" + masterTweenKind);
            }
            else
            {
#if UNITY_EDITOR
                for (int i = 0; i < masterTween.onFinished.GetPersistentEventCount(); i++)
                {
                    if (masterTween.onFinished.GetPersistentMethodName(i) == "OnFinishMasterTween")
                    {
                        Debug.LogWarning("OnFinishMasterTweenは自動で追加されるので削除してください");
                    }
                }
#endif
                masterTween.onFinished.RemoveListener(OnFinishMasterTween); // 旧仕様で使用している可能性があるので.
                masterTween.onFinished.AddListener(OnFinishMasterTween);

                // 自動で閉じるイベントは有効時にチェックする.
                if (masterTween.enabled)
                {
                    if (autoClose)
                    {
                        autoCloseState = AutoCloseState.CLOSE_REQUESTED;
                        tweenStartTime = Time.timeSinceLevelLoad;
                    }
                }
            }
        }

        /// <summary>
        /// 更新.
        /// </summary>
        void Update()
        {
            if (autoClose && autoCloseState == AutoCloseState.CLOSE_REQUESTED)
            {
                if (Time.timeSinceLevelLoad - tweenStartTime > autoCloseTime)
                {
                    autoCloseState = AutoCloseState.CLOSING;
                    Play(false);
                }
            }
        }

        /// <summary>
        /// プレイする.
        /// </summary>
        public void Play(bool isForward)
        {
            if (isForward)
            {
                if (autoClose)
                {
                    autoCloseState = AutoCloseState.CLOSE_REQUESTED;
                    tweenStartTime = Time.timeSinceLevelLoad;
                }
                gameObject.SetActive(true);
                SetButtonDisabled();
                foreach (uTweener tween in tweens)
                {
                    tween.Play(PlayDirection.Forward);
                }
            }
            else
            {
                SetButtonDisabled();
                foreach (uTweener tween in tweens)
                {
                    tween.Play(PlayDirection.Reverse);
                }
            }
        }

        /// <summary>
        /// GameObjectの表示・非表示を切り替えるマスターのTweenが終了したとき.
        /// </summary>
        public void OnFinishMasterTween()
        {
            if (masterTween.factor == 0.0f)
            {
                gameObject.SetActive(false);
                if (autoCloseState == AutoCloseState.CLOSING)
                {
                    if (onFinishAutoClose != null)
                    {
                        onFinishAutoClose.Invoke();
                    }
                    autoCloseState = AutoCloseState.NONE;
                }
            }
            else
            {
                SetButtonEnabled();
            }
        }

        /// <summary>
        /// トグル情報でフェードを管理する(インスペクタ用).
        /// </summary>
        public void Toggle(Toggle toggle)
        {
            Play(toggle.isOn);
        }

        /// <summary>
        /// ボタンを有効に.
        /// </summary>
        public void SetButtonEnabled()
        {
            foreach (Selectable select in GetComponentsInChildren<Selectable>())
            {
                select.enabled = true;
            }
        }

        /// <summary>
        /// ボタンを無効に.
        /// </summary>
        public void SetButtonDisabled()
        {
            foreach (Selectable select in GetComponentsInChildren<Selectable>())
            {
                select.enabled = false;
            }
        }
#endif
    }
}