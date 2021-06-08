#if LOG_TRACE && DEBUG
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace VMUnityLib
{
    /// <summary>
    /// ログ出力のラッパー。厳密にログ解析したいときに使う
    /// </summary>
    public sealed class VMLogger
    {
        class ObjectLog : IEnumerable
        {
            UnityEngine.Object target;
            // 上から順番に入っていって消えていく
            Queue<object> messageQueue = new Queue<object>();
            public void AddMessage(object message)
            {
                messageQueue.Enqueue(message);
            }
            public void DequeueMessage()
            {
                messageQueue.Dequeue();
            }
            public IEnumerator GetEnumerator()
            {
                foreach (var item in messageQueue)
                {
                    yield return item;
                }
            }
            public ObjectLog(UnityEngine.Object obj)
            {
                target = obj;
            }
        }
        static Dictionary<UnityEngine.Object, ObjectLog> messageStackDict = new Dictionary<UnityEngine.Object, ObjectLog>();
        static string dumpFilePath = null;
        static string dumpDirectoryPath = null;
        static Queue<ObjectLog> messageAddQueue = new Queue<ObjectLog>();   // 追加した順番を記録
        static int maxQueueSize = 1000;       // ログできるメッセージの総数
        // NOTE: ログ出来るオブジェクトは無限設定なので、シーン移動のタイミング等ゲーム側で適宜クリアする必要がある

        /// <summary>
        /// ダンプパス設定(拡張子、ファイルとディレクトリの隙間の/は不要)
        /// </summary
        static public void SetDumpDirectoryPath(string path)
        {
            dumpDirectoryPath = path;
        }
        static public void SetDumpFilePath(string path)
        {
            dumpFilePath = path;
        }
        
        /// <summary>
        /// ダンプする最大ログ量を設定
        /// </summary>
        static public void SetDumpMaxLines(int lines)
        {
            maxQueueSize = lines;
            RemoveOverQueues();
        }

        /// <summary>
        /// ログのダンプ
        /// </summary>
        /// <param name="clearLogStack">ログスタックをクリアするか</param>
        static public void DumpAll(bool clearLogStack = true)
        {
            string dumpStr = "";
            foreach (var keyObj in messageStackDict.Keys)
            {
                dumpStr += GetDumpStr(keyObj);
            }
            Debug.Log(dumpStr);
            DumpExternal(dumpStr);
            if (clearLogStack)
            {
                ClearLogStack();
            }
        }
        static public void Dump(UnityEngine.Object target)
        {
            string dumpStr = GetDumpStr(target);
            Debug.Log(dumpStr);
            DumpExternal(dumpStr);
        }
        static string GetDumpStr(UnityEngine.Object target)
        {
            string dumpStr = "";
            if(messageStackDict.ContainsKey(target))
            {
                dumpStr += "---------[" + target.name + "("+ target.GetInstanceID() +")]---------\n";
                var obj = messageStackDict[target];
                foreach (var item in obj)
                {
                    dumpStr += item + "\n";
                }
                dumpStr += "-------------------------------------\n";
            }
            return dumpStr;
        }

        /// <summary>
        /// 外部出力
        /// </summary>
        static void DumpExternal(string dumpStr)
        {
            if (!string.IsNullOrEmpty(dumpFilePath) && !string.IsNullOrEmpty(dumpFilePath))
            {
                try
                {
                    var now = DateTime.Now.ToString();
                    now = now.Replace(" ","_");
                    now = now.Replace("/","");
                    now = now.Replace(":","-");
                    var filePath = dumpDirectoryPath + "/" + dumpFilePath + "_" + now + ".txt";
                    File.WriteAllText(filePath, dumpStr);
#if UNITY_EDITOR
                    EditorUtility.RevealInFinder(filePath);
#endif
                }
                catch (IOException e)
                {
                    Debug.LogError(e.Message);
                }
            }
#if UNITY_EDITOR
            else if(EditorApplication.isPlaying)
#else
            else
#endif
            {
                Debug.LogError("SetDumpXXXPathで外部出力パスを設定してください。");
            }
        }

        /// <summary>
        /// ログのクリア
        /// </summary>
        static public void ClearLogStack()
        {
            messageStackDict.Clear();
        }

        /// <summary>
        /// ログ本体
        /// </summary>
        static public void Error(string message, UnityEngine.Object context, bool addStack = true, bool disableConsole = false)
        {
            if (!disableConsole)
            {
                if (!context) Debug.LogError(message);
                else Debug.LogError(message, context);
            }
            if (addStack)
            {
                AddLogStack(message, context);
                // エラー時はダンプする
                Dump(context);
            }
        }
        static public void Exception(Exception exp, UnityEngine.Object context, bool addStack = true, bool disableConsole = false)
        {
            if (!disableConsole)
            {
                if (!context) Debug.LogException(exp);
                else Debug.LogException(exp, context);
            }
            if (addStack) AddLogStack(exp.Message, context);
        }
        static public void Warn(string message, UnityEngine.Object context, bool addStack = true, bool disableConsole = false)
        {
            if (!disableConsole)
            {
                if (!context) Debug.LogWarning(message);
                else Debug.LogWarning(message, context);
            }
            if (addStack) AddLogStack(message, context);
        }
        static public void Log(string message, UnityEngine.Object context, bool addStack = true, bool disableConsole = false)
        {
            if (!disableConsole)
            {
                if (!context) Debug.Log(message);
                else Debug.Log(message, context);
            }
            if (addStack) AddLogStack(message, context);
        }

        /// <summary>
        /// ログスタック追加
        /// </summary>
        static public void AddLogStack(string message, UnityEngine.Object context)
        {
            if (!messageStackDict.ContainsKey(context))
            {
                messageStackDict.Add(context, new ObjectLog(context));
            }
            DateTime now = DateTime.Now;
            string timeStr = now.ToString() + ":" + now.Millisecond;
            var target = messageStackDict[context];
            var stackTraceStr = StackTraceUtility.ExtractStackTrace();

            // スタックトレース3行削除
            for (int i = 0; i < 3; i++)
            {
                var index = stackTraceStr.IndexOf("\n");
                stackTraceStr = stackTraceStr.Remove(0, index + 1);
            }

            target.AddMessage(timeStr + ": " + message + "\n" + stackTraceStr);
            messageAddQueue.Enqueue(target);
            RemoveOverQueues();
        }

        /// <summary>
        /// 範囲外キューの削除
        /// </summary>
        static void RemoveOverQueues()
        {
            int queueNum = messageAddQueue.Count;
            if (queueNum > maxQueueSize)
            {
                int dequeueNum = queueNum - maxQueueSize;
                for (int i = 0; i < dequeueNum; i++)
                {
                    var dequeue = messageAddQueue.Dequeue();
                    // 順番を保存して上から消せば入った順に消える
                    dequeue.DequeueMessage();
                }
            }
        }
    }
}
#endif