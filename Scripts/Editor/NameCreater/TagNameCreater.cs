using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace VMUnityLib
{
    /// <summary>
    /// タグ名を定数で管理するクラスを作成するスクリプト
    /// </summary>
    public static class TagNameCreator
    {
        // 無効な文字を管理する配列
        private static readonly string[] INVALUD_CHARS =
        {
            " ", "!", "\"", "#", "$",
            "%", "&", "\'", "(", ")",
            "-", "=", "^",  "~", "\\",
            "|", "[", "{",  "@", "`",
            "]", "}", ":",  "*", ";",
            "+", "/", "?",  ".", ">",
            ",", "<"
        };
    
        private const string ITEM_NAME  = "Tools/Create/Tag Name";  // コマンド名
        private const string PATH       = "Assets/MyGameAssets/LibBridge/Scripts/Names/TagName.cs";      // ファイルパス
    
        private static readonly string FILENAME                     = Path.GetFileName(PATH);                   // ファイル名(拡張子あり)
        private static readonly string FILENAME_WITHOUT_EXTENSION   = Path.GetFileNameWithoutExtension(PATH);   // ファイル名(拡張子なし)
    
        /// <summary>
        /// タグ名を定数で管理するクラスを作成します
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
        
            builder.AppendLine("using System.Collections.Generic;");
            builder.AppendLine("/// <summary>");
            builder.AppendLine("/// タグ名を定数で管理するクラス");
            builder.AppendLine("/// </summary>");
            builder.AppendFormat("public static class {0}", FILENAME_WITHOUT_EXTENSION).AppendLine();
            builder.AppendLine("{");
        
            foreach (var n in InternalEditorUtility.tags.
                     Select(c => new { var = RemoveInvalidChars(c), val = c }))
            {
                builder.Append("\t").AppendFormat(@"public const string {0} = ""{1}"";", n.var, n.val).AppendLine();
            }
        
            builder.Append("\t").AppendLine("");
            builder.Append("\t").AppendLine("/// <summary>");
            builder.Append("\t").AppendLine("/// タグ名の配列を取得");
            builder.Append("\t").AppendLine("/// </summary>");
            builder.Append("\t").AppendLine("public static string[] GetTagNames()");
            builder.Append("\t").AppendLine("{");
            builder.Append("\t").AppendLine("\tList<string> tagNames = new List<string>();");
            foreach (var n in InternalEditorUtility.tags.
                     Select(c => new { var = RemoveInvalidChars(c), val = c }))
            {
                builder.Append("\t\t").AppendFormat(@"tagNames.Add(TagName.{0}", n.var).Append(");").AppendLine();
            }
            builder.Append("\t\t").AppendLine("return tagNames.ToArray();");
            builder.Append("\t").AppendLine("}");
        
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
        /// タグ名を定数で管理するクラスを作成できるかどうかを取得します
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