/******************************************************************************/
/*!    \brief  ログ出力のラッパー。厳密にログ解析したいときに使う（コレを使うとUnityからダブルクリックでソースに飛べない）.
*******************************************************************************/

#define LOG_TRACE

using UnityEngine;
using System;

namespace VMUnityLib
{
    public sealed class Logger
    {
        static public void Error(string str)
        {
            Debug.LogError(str);
        }
        static public void Error(string str, UnityEngine.Object context)
        {
            Debug.LogError(str, context);
        }
        static public void Exception(Exception exp)
        {
            Debug.LogException(exp);
        }
        static public void Warn(string str)
        {
            Debug.LogWarning(str);
        }
        static public void Warn(string str, UnityEngine.Object context)
        {
            Debug.LogWarning(str, context);
        }
        static public void Log(string str)
        {
            Debug.Log(str);
        }
    }
}