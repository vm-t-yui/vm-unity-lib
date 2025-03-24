using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// ビルド前に特定の `Resources` 内のファイルを削除し、ビルド後に復元する
/// 除外ファイルリストは `ProjectSettings/ExcludeResources.json` で管理
/// 追加で色々書きたい場合はpartialつかってください
/// </summary>
public class CustomBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder => 0; // 実行順を制御（0で最優先）

    private static string settingsPath = "ProjectSettings/ExcludeResources.json"; // 設定ファイルのパス
    private static string backupFolder = "Assets/Editor/ExcludeResourcesBackup/"; // バックアップフォルダ

    /// <summary>
    /// JSON 形式の設定ファイルを読み込む
    /// </summary>
    private static List<string> LoadExcludeFileList()
    {
        if (!File.Exists(settingsPath))
        {
            Debug.LogWarning($"Exclude file list not found: {settingsPath}");
            return new List<string>();
        }

        string json = File.ReadAllText(settingsPath);
        ExcludeResourcesData data = JsonUtility.FromJson<ExcludeResourcesData>(json);
        return data?.excludeFiles ?? new List<string>();
    }

    /// <summary>
    /// ビルド前に `Resources` フォルダ内の特定ファイルをバックアップフォルダへ移動
    /// </summary>
    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("Excluding Resources files before build...");

        if (!Directory.Exists(backupFolder))
        {
            Directory.CreateDirectory(backupFolder);
        }

        List<string> excludeFiles = LoadExcludeFileList();
        foreach (string file in excludeFiles)
        {
            MoveFileToBackup(file);
        }

        AssetDatabase.Refresh();
    }

    /// <summary>
    /// ビルド後に `Resources` フォルダ内の特定ファイルを元に戻す
    /// </summary>
    public void OnPostprocessBuild(BuildReport report)
    {
        Debug.Log("Restoring Resources files after build...");

        List<string> excludeFiles = LoadExcludeFileList();
        foreach (string file in excludeFiles)
        {
            RestoreFileFromBackup(file);
        }

        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 指定したファイルを `Backup` フォルダに移動（.meta ファイルも含む）
    /// </summary>
    private static void MoveFileToBackup(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        string backupPath = Path.Combine(backupFolder, Path.GetFileName(filePath));
        File.Move(filePath, backupPath);
        Debug.Log($"Moved {filePath} to {backupPath}");

        // .meta ファイルも移動
        string metaFile = filePath + ".meta";
        string backupMetaPath = Path.Combine(backupFolder, Path.GetFileName(metaFile));
        if (File.Exists(metaFile))
        {
            File.Move(metaFile, backupMetaPath);
            Debug.Log($"Moved {metaFile} to {backupMetaPath}");
        }
    }

    /// <summary>
    /// `Backup` フォルダに移動したファイルを元の場所へ戻す
    /// </summary>
    private static void RestoreFileFromBackup(string filePath)
    {
        string backupPath = Path.Combine(backupFolder, Path.GetFileName(filePath));
        if (File.Exists(backupPath))
        {
            File.Move(backupPath, filePath);
            Debug.Log($"Restored {backupPath} to {filePath}");
        }

        // .meta ファイルも復元
        string metaFile = filePath + ".meta";
        string backupMetaPath = Path.Combine(backupFolder, Path.GetFileName(metaFile));
        if (File.Exists(backupMetaPath))
        {
            File.Move(backupMetaPath, metaFile);
            Debug.Log($"Restored {backupMetaPath} to {metaFile}");
        }
    }

    /// <summary>
    /// JSON のデータクラス
    /// </summary>
    [System.Serializable]
    private class ExcludeResourcesData
    {
        public List<string> excludeFiles;
    }
}
