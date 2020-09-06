/******************************************************************************/
/*!    \brief  標準MonoBehaviour（便利機能をラップしたMonoBehaviour）.
*******************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VMUnityLib
{
    public abstract class CmnMonoBehaviour : MonoBehaviour 
    {
        public const string INIT_SCENCE_CHANGE_NAME   = "InitSceneChange";
        public const string FADE_END_NAME             = "OnFadeInEnd";
        public const string SCENCE_DEACTIVE_NAME      = "OnSceneDeactive";

        /// <summary>
        /// ログをスタック記録するかどうか
        /// </summary>
        public bool EnableLogStack { get; set; } = true;

        /// <summary>
        /// ログを表示するかどうか
        /// </summary>
        public bool ShowLog { get; set; } = true;

        /// <summary>
        /// シーン切り替え後の初期化.
        /// </summary>
        protected virtual void InitSceneChange() { }

        /// <summary>
        /// シーンが無効になったとき.
        /// </summary>
        protected virtual void OnSceneDeactive() { }

        /// <summary>
        /// フェードイン終了後の処理.
        /// </summary>
        protected virtual void OnFadeInEnd() { }

        /// <summary>
        /// コルーチン
        /// </summary>
        class Coroutines
        {
            public Coroutine coroutine;
            public string enumratorId;
            public IEnumerator enumrator;
        }
        List<Coroutines> coroutines = new List<Coroutines>();
        public Coroutine SafeStartCoroutine(IEnumerator routine)
        {
            string id = routine.ToString();
            var newCoroutine = new Coroutines();
            newCoroutine.enumratorId = id;
            newCoroutine.enumrator = routine;
            coroutines.Add(newCoroutine);
            var ret = StartCoroutine(InnnerCoroutineWrapper(routine, id));
            newCoroutine.coroutine = ret;
            return ret;
        }
        private IEnumerator InnnerCoroutineWrapper(IEnumerator routine, string id)
        {
            yield return routine;
            int length = coroutines.Count;
            for (int i = 0; i < length; i++)
            {
                if(coroutines[i].enumratorId == id)
                {
                    coroutines.RemoveAt(i);
                    break;
                }
            }
        }
        public void SafeStopAllCoroutines()
        {
            StopAllCoroutines();
            coroutines.Clear();
        }
        public void SafeStopCoroutine(Coroutine routine)
        {
            StopCoroutine(routine);
            int length = coroutines.Count;
            for (int i = 0; i < length; i++)
            {
                if (coroutines[i].coroutine == routine)
                {
                    coroutines.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// 起動中コルーチン名取得
        /// </summary>
        public string GetRunningCoroutinesName()
        {
            string ret = "";
            foreach (var item in coroutines)
            {
                ret += item.enumratorId + "\n";
            }
            return ret;
        }

#if DEBUG && LOG_TRACE
        /// <summary>
        /// ログ
        /// </summary>
        public void LogError(string message)
        {
            VMLogger.Error(message, this, EnableLogStack, !ShowLog);
        }
        public void LogException(Exception exp)
        {
            VMLogger.Exception(exp, this, EnableLogStack, !ShowLog);
        }
        public void LogWarn(string message)
        {
            VMLogger.Warn(message, this, EnableLogStack, !ShowLog);
        }
        public void Log(string message)
        {
            VMLogger.Log(message, this, EnableLogStack, !ShowLog);
        }
        public void DumpLog()
        {
            VMLogger.Dump(this);
        }
#endif
    }
}