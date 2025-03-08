using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        ApplyFrameRate(TargetFrameRate, FixedDeltaTime);
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
    public static void ApplyQuallitySetting(int qualityLevel)
    {
        QualitySettings.SetQualityLevel(qualityLevel);
        ApplyFrameRateFromSaveData();   // Unityが設定吹き飛ばすのでフレームレート更新
    }

    /// <summary>
    /// 解像度を設定
    /// </summary>
    public static void ApplyResolution(Vector2Int resolution, bool isWindowMode)
    {
        Screen.SetResolution(resolution.x, resolution.y, !isWindowMode);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"change resolution : [w:{resolution.x}] [y:{resolution.y}] [isWindow:{isWindowMode}]");
#endif
        ApplyFrameRateFromSaveData();   // Unityが設定吹き飛ばすのでフレームレート更新
    }

    /// <summary>
    /// ロードブースト要のグラフィクス設定
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
