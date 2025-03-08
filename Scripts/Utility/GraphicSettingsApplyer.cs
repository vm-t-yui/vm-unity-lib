using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// グラフィクス設定を適用させるクラス
/// </summary>
public class GraphicSettingsApplyer
{
#if UNITY_GAMECORE_XBOXONE || UNITY_PS4 || UNITY_SWITCH || YUONI_SWITCH
    public static int TargetFrameRate = 30;
    public static float FixedDeltaTime = 1.0f / 30.0f;
#else
    public static int TargetFrameRate = 60;
    public static float FixedDeltaTime = 1.0f / 60.0f;
#endif
    public static int LoadBoostingFrameRate = 10;

    // 解像度
    // マルチモニタ環境だとリフレッシュレート（モニタ）ごとに解像度が保存されてしまうため、
    // 適用可能な解像度をリフレッシュレート無視で保存
    public static List<Vector2Int> AppliableResolutions { get; private set; }
    public static int AppliableResolutionsLength { get { return AppliableResolutions.Count; } }

    /// <summary>
    /// 全設定をセーブデータから読み込む
    /// </summary>
    public static void ApplyAllSettingsFromSaveData()
    {
        ApplyResolution(new Vector2Int(SaveDataManager.Data.ResolutionW, SaveDataManager.Data.ResolutionH), SaveDataManager.Data.IsWindowMode, false);
        ApplyQuallitySetting((int)SaveDataManager.Data.Quality, true); // クオリティ設定決めてフレームレートも更新する
    }

    /// <summary>
    /// 適用可能解像度更新
    /// </summary>
    public static void UpdateAppliableResolutions()
    {
        // リフレッシュレートとは関係ない適用可能な重複しない解像度を作成
        var length = Screen.resolutions.Length;
        AppliableResolutions = new List<Vector2Int>();
        for (int i = 0; i < length; i++)
        {
            var newResolution = Screen.resolutions[i];
            if (!AppliableResolutions.Any(resolution => (resolution.x == newResolution.width && resolution.y == newResolution.height)))
            {
                AppliableResolutions.Add(new Vector2Int(newResolution.width, newResolution.height));
            }
        }
    }


    /// <summary>
    /// インデックスから重複しない適用可能解像度を取得する
    /// </summary>
    public static Vector2Int GetResolutionByIndex(int index)
    {
        // HACK: Vector2Intなのは、リフレッシュレートはこの関数でいじらせないぞという意思表示
        var resolution = Screen.resolutions[index];
        return new Vector2Int(resolution.width, resolution.height);
    }

    /// <summary>
    /// セーブデータ状態からフレームレートを反映
    /// </summary>
    public static void ApplyFrameRateFromSaveData()
    {
#if UNITY_GAMECORE_XBOXONE || UNITY_PS4 || UNITY_SWITCH || YUONI_SWITCH
        ApplyFrameRate(TargetFrameRate, FixedDeltaTime);
#else
        int targetFrameRate = TargetFrameRate;
        float fixedDeltaTime = FixedDeltaTime;
        if(SaveDataManager.Data != null && SaveDataManager.Data.Is30Fps)
        {
            targetFrameRate = 30;
            fixedDeltaTime = 1.0f / 30.0f;
        }
        ApplyFrameRate(targetFrameRate, fixedDeltaTime);
#endif

        // ロード高速化のためにCPUブースト戻す
#if !UNITY_EDITOR && !UNITY_SWITCH && !UNITY_STANDALONE_WIN
        UnityEngine.Switch.Performance.SetCpuBoostMode(UnityEngine.Switch.Performance.CpuBoostMode.Normal);
#endif
        Application.backgroundLoadingPriority = ThreadPriority.Low;
    }

    /// <summary>
    /// フレームレート更新
    /// </summary>
    public static void ApplyFrameRate(int targetFrameRate, float fixedDeltaTime)
    {
        // プラットフォームにあわせて画面設定を変える
#if UNITY_GAMECORE_XBOXONE || UNITY_SWITCH || YUONI_SWITCH
        QualitySettings.vSyncCount = 2;
        QualitySettings.streamingMipmapsActive = false; // コンスタントに2.5ms程消費するので使わない
        // トリプルバッファリング無効化
        QualitySettings.maxQueuedFrames = 1;
#elif UNITY_PS4
        QualitySettings.vSyncCount = 2;
#elif UNITY_STANDALONE_WIN
        QualitySettings.vSyncCount = 0; // 非コンソールは垂直同期を切る
#else
        QualitySettings.vSyncCount = 1;
#endif
        Application.targetFrameRate = targetFrameRate;
        Time.fixedDeltaTime = fixedDeltaTime;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"change framerate : [rate:{Application.targetFrameRate}] [delta time:{Time.fixedDeltaTime}] [vsync coount:{QualitySettings.vSyncCount}]");
#endif
    }

    /// <summary>
    /// クオリティ設定更新
    /// </summary>
    public static void ApplyQuallitySetting(int qualityLevel, bool applyFrameRateFromSaveData = true)
    {
        QualitySettings.SetQualityLevel(qualityLevel);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"change qualityLevel : [{qualityLevel}] ");
#endif
        if (applyFrameRateFromSaveData)
        {
            ApplyFrameRateFromSaveData();   // Unityが設定吹き飛ばすのでデフォルトはフレームレート更新
        }
    }

    /// <summary>
    /// 解像度を設定
    /// </summary>
    public static void ApplyResolution(Vector2Int resolution, bool isWindowMode, bool applyFrameRateFromSaveData = true)
    {
        // HACK: Vector2Intなのは、リフレッシュレートはこの関数でいじらせないぞという意思表示
        // 指定解像度が設定可能かチェック。なければデフォルト解像度に
        // 元々がウインドウモードかどうかはこの時点で関係がない情報なので、全データから見る
        var length = Screen.resolutions.Length;
        bool found = false;
        for (int i = 0; i < length; i++)
        {
            var serarchResolution = Screen.resolutions[i];
            if (serarchResolution.width == resolution.x && serarchResolution.height == resolution.y)
            {
                found = true;
                break;
            }
        }
        if(!found)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // 見つからなかったらエラー吐いて最大解像度にする。マルチディスプレイでも大丈夫
            Debug.LogWarning($"invalid resolution : [w:{resolution.x}] [y:{resolution.y}] [isWindow:{isWindowMode}]");
#endif
            int maxIndex = Screen.resolutions.Length - 1;
            resolution.x = Screen.resolutions[maxIndex].width;
            resolution.y = Screen.resolutions[maxIndex].height;
        }
        Screen.SetResolution(resolution.x, resolution.y, !isWindowMode);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"change resolution : [w:{resolution.x}] [y:{resolution.y}] [isWindow:{isWindowMode}]");
#endif
        if(applyFrameRateFromSaveData)
        {
            ApplyFrameRateFromSaveData();   // Unityが設定吹き飛ばすのでフレームレート更新
        }
    }

    /// <summary>
    /// ロードブースト用のグラフィクス設定
    /// </summary>
    public static void ApplyLoadingBoostSetting()
    {
        ApplyFrameRate(LoadBoostingFrameRate, 1.0f / LoadBoostingFrameRate);

        // ロード高速化のためにCPUブースト
#if !UNITY_EDITOR && !UNITY_SWITCH && !UNITY_STANDALONE_WIN
        UnityEngine.Switch.Performance.SetCpuBoostMode(UnityEngine.Switch.Performance.CpuBoostMode.FastLoad);
#endif
        Application.backgroundLoadingPriority = ThreadPriority.High;
    }

}
