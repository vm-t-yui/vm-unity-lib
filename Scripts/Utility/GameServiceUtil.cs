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
#if UNITY_ANDROID
    public const string LEADERBOARD_ID = GPGSIds.leaderboard_extirpation_ranking;
#else
    public const string  LEADERBOARD_ID = "btmankick.rank";
#endif

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
    // 
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
    public static void ReportScore(long score)
    {
#if !UNITY_EDITOR
        Social.ReportScore(score, LEADERBOARD_ID, success => 
        {
            if(!success)
            {
                Debug.LogWarning("スコア報告に失敗しました。id:"+LEADERBOARD_ID);
            }
        });
#endif
    }
}
