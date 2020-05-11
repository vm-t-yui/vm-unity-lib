using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace VMUnityLib
{
    /// <summary>
    /// シーン名のEnumとテーブルを作成するクラス
    /// </summary>
    public static class SceneNameTableCreator
    {
        // 無効な文字を管理する配列
        static readonly string[] INVALUD_CHARS =
        {
            " ", "!", "\"", "#", "$",
            "%", "&", "\'", "(", ")",
            "-", "=", "^",  "~", "\\",
            "|", "[", "{",  "@", "`",
            "]", "}", ":",  "*", ";",
            "+", "/", "?",  ".", ">",
            ",", "<"
        };
    
        const string ITEM_NAME  = "Tools/Create/Scene Name";    // コマンド名
        const string PATH       = "Assets/MyGameAssets/LibBridge/Scripts/Names/SceneName.cs";        // ファイルパス
    
        static readonly string FILENAME                     = Path.GetFileName(PATH);                   // ファイル名(拡張子あり)
        static readonly string FILENAME_WITHOUT_EXTENSION   = Path.GetFileNameWithoutExtension(PATH);   // ファイル名(拡張子なし)
    
        /// <summary>
        /// シーン名を定数で管理するクラスを作成します
        /// </summary>
        [MenuItem(ITEM_NAME)]
        public static void Create()
        {
            if (!CanCreate())
            {
                return;
            }
        
            CreateScript();
        
            EditorUtility.DisplayDialog(FILENAME, "作成が完了しました", "OK");
        }
    
        /// <summary>
        /// スクリプトを作成します
        /// </summary>
        public static void CreateScript()
        {
            var builder = new StringBuilder();

            // using
            builder.AppendLine("using System.Collections.Generic;");

            // クラス名とクラスコメント
            builder.AppendLine("/// <summary>");
            builder.AppendLine("/// シーン名を定数で管理するクラス");
            builder.AppendLine("/// </summary>");
            builder.AppendFormat("public static class {0}", FILENAME_WITHOUT_EXTENSION).AppendLine();
            builder.AppendLine("{");

            // Enumを作成する
            CreateEnum(builder);

            // 改行
            builder.AppendLine();

            // 定数を作成する
            CreateConstantValue(builder);

            // 改行
            builder.AppendLine();

            // Dictionaryのテーブルを作成
            CreateDictionaryTable(builder);

            builder.AppendLine("}");
        
            var directoryName = Path.GetDirectoryName(PATH);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
        
            File.WriteAllText(PATH, builder.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);
        }

        /// <summary>
        /// Enumを作成する
        /// </summary>
        static void CreateEnum(StringBuilder builder)
        {
            // Enum名とコメント
            builder.Append("\t").AppendLine("/// <summary>");
            builder.Append("\t").AppendLine("/// シーンの種類");
            builder.Append("\t").AppendLine("/// </summary>");
            builder.Append("\t").AppendFormat("public enum {0}Kind", FILENAME_WITHOUT_EXTENSION).AppendLine();
            builder.Append("\t").AppendLine("{");

            // メンバー
            foreach (var n in EditorBuildSettings.scenes
                     .Select(c => Path.GetFileNameWithoutExtension(c.path))
                     .Distinct()
                     .Select(c => RemoveInvalidChars(c).SnakeToUpperCamel()))
            {
                builder.Append("\t\t").AppendFormat(@"{0},", n).AppendLine();
            }

            builder.Append("\t").AppendLine("}");
        }

        /// <summary>
        /// 定数を作成する
        /// </summary>
        static void CreateConstantValue(StringBuilder builder)
        {
            foreach (var n in EditorBuildSettings.scenes
                     .Select(c => Path.GetFileNameWithoutExtension(c.path))
                     .Distinct()
                     .Select(c => new { var = RemoveInvalidChars(c).SnakeToUpperCamel(), val = c }))
            {
                builder.Append("\t").AppendFormat(@"public const string {0} = ""{1}"";", n.var, n.val).AppendLine();
            }
        }

        /// <summary>
        /// Dictionaryのテーブルを作成
        /// </summary>
        static void CreateDictionaryTable(StringBuilder builder)
        {
            // Dictionary名とコメント
            builder.Append("\t").AppendLine("/// <summary>");
            builder.Append("\t").AppendLine("/// シーンの名前テーブル");
            builder.Append("\t").AppendLine("/// </summary>");
            builder.Append("\t").AppendFormat("public static readonly IReadOnlyDictionary<{0}Kind, string> {0}Table = new Dictionary<{0}Kind, string>()", FILENAME_WITHOUT_EXTENSION).AppendLine();
            builder.Append("\t").AppendLine("{");

            // メンバー
            foreach (var n in EditorBuildSettings.scenes
                     .Select(c => Path.GetFileNameWithoutExtension(c.path))
                     .Distinct()
                     .Select(c => new { var = RemoveInvalidChars(c).SnakeToUpperCamel(), val = c }))
            {
                builder.Append("\t\t").AppendFormat("{{{0}Kind.{1}, \"{2}\"}},", FILENAME_WITHOUT_EXTENSION, n.var, n.val).AppendLine();
            }

            builder.Append("\t").AppendLine("};");
        }
    
        /// <summary>
        /// シーン名を定数で管理するクラスを作成できるかどうかを取得します
        /// </summary>
        [MenuItem(ITEM_NAME, true)]
        public static bool CanCreate()
        {
            return !EditorApplication.isPlaying && !Application.isPlaying && !EditorApplication.isCompiling;
        }
    
        /// <summary>
        /// 無効な文字を削除します
        /// </summary>
        public static string RemoveInvalidChars(string str)
        {
            Array.ForEach(INVALUD_CHARS, c => str = str.Replace(c, string.Empty));
            return str;
        }
    }
}