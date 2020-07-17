/******************************************************************************/
/*!    \brief  実績やリーダーボードなどのゲームサービスリティ.
*******************************************************************************/
using UnityEngine;
#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

public class GameServiceUtil
{
    //// NOTE: m.tanaka 下の各IDはLibBridgeファイル内にスクリプトを作成しています。
    //// 各実績ID
    //static string[] ACHIEVEMENT_IDs =
    //{
    //    GameServiceID.ACHIEVEMENT_1,
    //    GameServiceID.ACHIEVEMENT_2,
    //    GameServiceID.ACHIEVEMENT_3,
    //    GameServiceID.ACHIEVEMENT_4
    //};

    //// 各リーダーボードID
    //static string[] LEADERBOARD_IDs =
    //{
    //    GameServiceID.LEADERBOARD_1,
    //    GameServiceID.LEADERBOARD_2
    //};

    /// <summary>
    /// ユーザー認証.
    /// </summary>
    public static void Auth()
    {
#if UNITY_ANDROID
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
       .Build();

        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
#endif

        // 認証のため ProcessAuthenticationをコールバックとして登録
        // This call needs to be made before we can proceed to other calls in the Social API
        Social.localUser.Authenticate(ProcessAuthentication);
    }
    // 認証が完了した時に呼び出される
    // 認証が成功した場合、サーバーからのデータがSocial.localUserにセットされている
    static void ProcessAuthentication(bool success)
    {
        if (success)
        {
            Debug.Log("Authenticated, checking achievements" + Social.localUser.ToString());
        }
        else
        {
            Debug.Log("Failed to authenticate");
        }
    }

    /// <summary>
    /// リーダーボードを表示する.
    /// </summary>
    public static void ShowLeaderboardUI()
    {
        Social.ShowLeaderboardUI();
    }

    /// <summary>
    /// 実績表示.
    /// </summary>
    public static void ShowAchivementUI()
    {
        Social.ShowAchievementsUI();
    }

    /// <summary>
    /// リーダーボードにスコアを送信する.
    /// </summary>
    public static void ReportScore(long score, int leaderboardNum)
    {
#if !UNITY_EDITOR
        //Social.ReportScore(score, LEADERBOARD_IDs[leaderboardNum], success => 
        //{
        //    if(!success)
        //    {
        //        Debug.LogWarning("スコア報告に失敗しました。id:" + LEADERBOARD_IDs[leaderboardNum]);
        //    }
        //});
#endif
    }

    /// <summary>
    /// 実績を解除する
    /// </summary>
    public static void ReportProgress(int achievementNum)
    {
#if !UNITY_EDITOR
        //Social.ReportProgress(ACHIEVEMENT_IDs[achievementNum], 100, (bool success) =>
        //{
        //    if (!success)
        //    {
        //        Debug.LogWarning("実績解除に失敗しました。id:" + ACHIEVEMENT_IDs[achievementNum]);
        //    }
        //    else
        //    {
        //        // UI用の新規実績解除フラグをオン
        //        GameDataManager.Inst.PlayData.IsNewReleasedAchieve = true;
        //    }
        //});
#endif
    }
}
