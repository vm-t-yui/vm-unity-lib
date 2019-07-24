﻿/******************************************************************************/
/*!    \brief  データをJSONファイルで書き出す＆JSONファイルを読み込む.
*******************************************************************************/

using UnityEngine;
using System.IO;

namespace VMUnityLib
{
    public static class JsonDataSaver
    {
        /// <summary>
        /// セーブ
        /// </summary>
        /// <param name="saveData">セーブするデータ（クラス）</param>
        public static void Save<T>(T saveData)
            where T : class
        {
            // 文字コードを指定してテキストファイルに書き込むクラスをインスタンス化
            StreamWriter writer;

            // データをJSON形式に変換
            string jsonStr = JsonUtility.ToJson(saveData);
            string dataName = saveData.ToString();

            // JSONファイルを作成し、書き込み（クラスの名前のJSONファイルを作成）
            writer = new StreamWriter(Application.dataPath + "/MyGameAssets/SaveData/" + dataName + ".json", false);
            writer.Write(jsonStr);
            writer.Flush();
            writer.Close();
        }

        /// <summary>
        /// ロード
        /// </summary>
        /// <param name="loadData">ロードするデータ（クラス）</param>
        public static T Load<T>(T loadData)
            where T : class
        {
            // テキストファイルを読み込むクラスをインスタンス化
            StreamReader reader;

            // 読み込んだJSONファイルを保存する変数を用意
            string dataStr = "";
            string dataName = loadData.ToString();

            // JSONファイルを読み込み、string型で保存
            reader = new StreamReader(Application.dataPath + "/MyGameAssets/SaveData/" + dataName + ".json");
            dataStr = reader.ReadToEnd();
            reader.Close();

            // JSON形式からオブジェクトに変換して返す
            return JsonUtility.FromJson<T>(dataStr);
        }
    }
}