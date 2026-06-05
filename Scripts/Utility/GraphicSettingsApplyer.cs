#if YUONI_SWITCH
#define UNITY_SWITCH
#undef UNITY_STANDALONE_WIN
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// グラフィクス設定を適用させるクラス
/// </summary>
public class GraphicSettingsApplyer
{
    // 最小解像度
    const int MinResolutionWidth = 1280;
    const int MinResolutionHeight = 720;

    // 最小fps
    const float MinFps = 29.0f;
    const int MinVSyncCount = 4;

    // ロードブースト中のフレームレート
    const int LoadBoostingFrameRate = 10;

    // 解像度
    // マルチモニタ環境だとリフレッシュレート（モニタ）ごとに解像度が保存されてしまうため、
    // 適用可能な解像度をリフレッシュレート無視で保存
    public static List<Vector2Int> AppliableResolutions { get; private set; }
    public static int AppliableResolutionsLength { get { return AppliableResolutions.Count; } }

    // VSyncCount
    // 解像度と同じくVSyncCountも保存
    public static List<int> AppliableVSyncCount { get; private set; }
    public static int AppliableVSyncCountLength { get { return AppliableVSyncCount.Count; } }

    /// <summary>
    /// 全設定をセーブデータから読み込む
    /// </summary>
    public static void ApplyAllSettingsFromSaveData()
    {
        // クオリティ設定決めてフレームレートも更新する
        // セーブデータのフレームレートで復元しないのでクオリティ設定の方は解像度キープしない
        ApplyQuallitySetting((int)SaveDataManager.Data.Quality, false);
        ApplyResolution(new Vector2Int(SaveDataManager.Data.ResolutionW, SaveDataManager.Data.ResolutionH), SaveDataManager.Data.IsWindowMode, false);
    }

    /// <summary>
    /// 適用可能なグラフィックデータ更新
    /// </summary>
    public static void UpdateAppliableData()
    {
        // リフレッシュレートとは関係ない適用可能な重複しない解像度を作成
        var length = Screen.resolutions.Length;
        AppliableResolutions = new List<Vector2Int>();
        for (int i = 0; i < length; i++)
        {
            var newResolution = Screen.resolutions[i];
            if (!AppliableResolutions.Any(resolution => (resolution.x == newResolution.width && resolution.y == newResolution.height)))
            {
                // 最低解像度を越えるもの以外は無視
                if(newResolution.width >= MinResolutionWidth && newResolution.height >= MinResolutionHeight)
                {
                    AppliableResolutions.Add(new Vector2Int(newResolution.width, newResolution.height));
                }
            }
        }

        // 適用可能なvSyncCountを算定
        // 144fps合わせで最小24fpsとする
        var refreshRate = (float)Screen.currentResolution.refreshRateRatio.value;
        var targetRefreshRate = refreshRate;
        int count = 1;
        AppliableVSyncCount = new List<int>();
        while (targetRefreshRate > MinFps && count <= MinVSyncCount)
        {
            AppliableVSyncCount.Add(count);
            ++count;
            targetRefreshRate = refreshRate / count;
        }
    }

    /// <summary>
    /// インデックスから重複しない適用可能解像度を取得する
    /// </summary>
    public static Vector2Int GetResolutionByIndex(int index)
    {
        // HACK: Vector2Intなのは、リフレッシュレートはこの関数でいじらせないぞという意思表示
        var resolution = AppliableResolutions[index];
        return new Vector2Int(resolution.x, resolution.y);
    }

    /// <summary>
    /// セーブデータ状態からフレームレートを反映
    /// </summary>
    public static void ApplyFrameRateFromSaveData()
    {
#if UNITY_STANDALONE_WIN
        ApplyFrameRate(SaveDataManager.Data.VSyncCount);
#elif UNITY_SWITCH2
        // Switch2は60固定
        ApplyFrameRate(1, 60);
#else
        ApplyFrameRate(0);
#endif
        // ロード高速化のためにCPUブースト戻す
#if (UNITY_SWITCH || UNITY_SWITCH2) && !UNITY_EDITOR  && !UNITY_STANDALONE_WIN
        SwitchApis.SetCpuBoostMode(SwitchApis.CpuBoostMode.Normal);
#endif
        Application.backgroundLoadingPriority = ThreadPriority.Low;
    }

    /// <summary>
    /// フレームレート更新
    /// </summary>
    /// <param name="targetFrameRate">垂直同期しないときのターゲットフレームレート</param>
    public static void ApplyFrameRate(int vSyncCount, int targetFrameRate = 0)
    {
        // プラットフォームにあわせて同期画面設定を変える
        // コンソールは固定する
#if UNITY_STANDALONE_WIN
        QualitySettings.vSyncCount = vSyncCount;
#elif YUONI_SWITCH
        // 60FPS固定
        QualitySettings.vSyncCount = 1;
        targetFrameRate = 60;
#elif UNITY_GAMECORE_XBOXONE || UNITY_SWITCH || UNITY_PS4
        QualitySettings.vSyncCount = 2;
#else
        QualitySettings.vSyncCount = 1;
#endif
        Application.targetFrameRate = targetFrameRate;

        var refreshRate = Screen.currentResolution.refreshRateRatio.value;

        // 垂直同期切ってるときはTargetFrameRateから計算
        if(QualitySettings.vSyncCount == 0)
        {
            Time.fixedDeltaTime = 1.0f / (float)Application.targetFrameRate;
        }
        else
        {
            Time.fixedDeltaTime = 1.0f / (float)refreshRate;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"change framerate : [refreshRate:{refreshRate}] [targetFrameRate:{Application.targetFrameRate}] [delta time:{Time.fixedDeltaTime}] [vsync coount:{QualitySettings.vSyncCount}]");
#endif
    }

    /// <summary>
    /// クオリティ設定更新
    /// </summary>
    public static void ApplyQuallitySetting(int qualityLevel, bool keepFrameRate)
    {
        // NOTE: 
        // Switch2はドックの抜き差しによってクオリティを更新する
        // ・TVモード：4K、Switch本体設定側の解像度に関わらず、クオリティはLow固定
        // ・携帯モード：FullHD、クオリティはMiddle固定
        // 抜き差し検知は「QualityLevelAdjusterSwitch2」クラスで行う

        // SetResolutionでUnityがフレームレート設定吹き飛ばすのでVsync状態を保存しておいて計算しなおし
        int prevVSyncCount = QualitySettings.vSyncCount;

#if UNITY_SWITCH
        // スイッチは問答無用で落とす
        qualityLevel = 0;
#endif

        QualitySettings.SetQualityLevel(qualityLevel);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"change qualityLevel : [{qualityLevel}] ");
#endif
        if (keepFrameRate)
        {
            ApplyFrameRate(prevVSyncCount);
        }
    }

    /// <summary>
    /// 解像度を設定
    /// </summary>
    /// <param name="resolution">解像度</param>
    /// <param name="isWindowMode">ウインドウモード</param>
    /// <param name="applyFrameRateFromSaveData">フレームレートをキープするか</param>
    public static void ApplyResolution(Vector2Int resolution, bool isWindowMode, bool keepFrameRate)
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
        int prevVSyncCount = QualitySettings.vSyncCount;
        Screen.SetResolution(resolution.x, resolution.y, !isWindowMode);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"change resolution : [w:{resolution.x}] [y:{resolution.y}] [isWindow:{isWindowMode}]");
#endif
        // Unityが設定吹き飛ばすのでフレームレート更新
        if (keepFrameRate)
        {
            ApplyFrameRate(prevVSyncCount);
        }
    }

    /// <summary>
    /// ロードブースト用のグラフィクス設定
    /// </summary>
    public static void ApplyLoadingBoostSetting()
    {
        ApplyFrameRate(0, LoadBoostingFrameRate);

        // ロード高速化のためにCPUブースト
#if (UNITY_SWITCH || UNITY_SWITCH2) && !UNITY_EDITOR  && !UNITY_STANDALONE_WIN
        SwitchApis.SetCpuBoostMode(SwitchApis.CpuBoostMode.FastLoad);
#endif
        Application.backgroundLoadingPriority = ThreadPriority.High;
    }

}
