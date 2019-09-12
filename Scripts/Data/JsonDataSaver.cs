/******************************************************************************/
/*!    \brief  データをJSONファイルで書き出す＆JSONファイルを読み込む.
*******************************************************************************/

using UnityEngine;
using System.IO;

namespace VMUnityLib
{
    public static class JsonDataSaver
    {
        /// <summary>
        /// データのパスを生成
        /// </summary>
        static string CreateDataPath<T>(T data)
            where T : class
        {
            string dataName = "/" + data.ToString() + ".json";

            string dataPath =
#if UNITY_EDITOR
            Application.dataPath + "/SaveData" + dataName;
#else
            Application.persistentDataPath + dataName;
#endif

            return dataPath;
        }

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

            // JSONファイルを作成し、書き込み（クラスの名前のJSONファイルを作成）
            writer = new StreamWriter(CreateDataPath(saveData), false);
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

            // JSONファイルを読み込み、string型で保存
            reader = new StreamReader(CreateDataPath(loadData));
            dataStr = reader.ReadToEnd();
            reader.Close();

            // JSON形式からオブジェクトに変換して返す
            return JsonUtility.FromJson<T>(dataStr);
        }

        /// <summary>
        /// ファイルが存在するか
        /// </summary>
        /// <param name="checkData">調べるデータ（クラス）</param>
        /// <returns></returns>
        public static bool FileExists<T>(T checkData)
            where T : class
        {
            // 指定したファイルがあればtrueを返す
            if (File.Exists(CreateDataPath(checkData)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}