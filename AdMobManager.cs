#if ENABLE_ADMOB
#pragma warning disable 0414
/******************************************************************************/
/*!    \brief  広告の表示非表示
*******************************************************************************/
// 参考 http://games.genieus.co.jp/unity/admob_unity/

//== テスト用ID https://developers.google.com/admob/android/test-ads ======================//
// banner          : 機種共通 ca-app-pub-3940256099942544/6300978111
// interstitial    : 機種共通 ca-app-pub-3940256099942544/1033173712

// Rewarded Video  : Android  ca-app-pub-3940256099942544/5224354917
// Rewarded Video  : ios      ca-app-pub-3940256099942544/1712485313

// Native Advanced : Android  ca-app-pub-3940256099942544/2247696110
// Native Advanced : ios      ca-app-pub-3940256099942544/2247696110

// Native Express  : Android  (Small template): ca-app-pub-3940256099942544/2793859312
//                            (Large template): ca-app-pub-3940256099942544/2177258514
// Native Express  : ios      (Small template): ca-app-pub-3940256099942544/4270592515
//                            (Large template): ca-app-pub-3940256099942544/8897359316
//=========================================================================================//
// 必要な広告
//・偽ローディング画面（中央に表示されるネイティブアド大）
//・リザルト時および偽ロード時バナー（上下一個ずつ）
//・リザルト時インタースティシャル
//・おすすめアプリ時（ネイティブアド小）

// ※ネイティブ広告について
// 　admobが2018年3月に今のネイティブ広告を廃止。
//　 それに伴い新規広告枠の作成が不可能になっている

using System.Collections;
using UnityEngine;
using GoogleMobileAds.Api;
using VMUnityLib;
using System;
using UnityEngine.Advertisements;

public sealed class AdMobManager : SingletonMonoBehaviour<AdMobManager>
{
    public enum BANNER
    {
        TOP,
        BOTTOM,
        MAX
    }
    [SerializeField]
    public string[] Android_Banner = default;
    [SerializeField]
    public string Android_Interstitial = default;
    [SerializeField]
    public string Android_NativeExpress = default;
    [SerializeField]
    public string[] ios_Banner = default;
    [SerializeField]
    public string ios_Interstitial = default;
    [SerializeField]
    public string ios_NativeExpress = default;
    [SerializeField]
    Action rewardAction = default;
    [SerializeField]
    MyNendNative myNendNative = default;

    InterstitialAd interstitial;
    BannerView[]   banner = new BannerView[(int)BANNER.MAX];
    bool[]         beforeBanner = new bool[(int)BANNER.MAX];
    AdRequest          request;
    RewardBasedVideoAd rewardBasedVideo;

    bool is_close_interstitial = false;
    bool isMovieReward;


    // Use this for initialization
    void Awake()
    {
        // 起動時にインタースティシャル広告をロードしておく
        RequestInterstitial();
        // バナー広告を読み込み
        RequestBanner();
        // 中央広告読み込み
        RequestNativeExpress();
        // 動画広告読み込み
        RequestRewardBasedVideo();
        // UnityAdsの初期化
        InitializeUnityAds();
    }

    void InitializeUnityAds()
    {
        //初期化
#if UNITY_ANDROID
        Advertisement.Initialize("3896591");
#elif UNITY_IOS
        Advertisement.Initialize("3896590");
#else
#endif
    }

    void RequestRewardBasedVideo()
    {
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-7073050807259252/1483640786";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-7073050807259252/8534643605";
#else
        string adUnitId = "unexpected_platform";
#endif

        rewardBasedVideo = RewardBasedVideoAd.Instance;
        rewardBasedVideo.OnAdRewarded += OnAdRewarded;
        rewardBasedVideo.OnAdClosed   += OnAdClosed;

        AdRequest request = new AdRequest.Builder().Build();
        rewardBasedVideo.LoadAd(request, adUnitId);
    }

    /// <summary>
    /// バナー読み込み
    /// </summary>
    public void RequestBanner()
    {
        string[] id = new string[(int)BANNER.MAX];
#if UNITY_ANDROID
        for(int i = 0; i < (int)BANNER.MAX; i++)
        {
            id[i] = Android_Banner[i];
        }
#elif UNITY_IPHONE
        for(int i = 0; i < (int)BANNER.MAX; i++)
        {
            id[i] = ios_Banner[i];
        }
#else
        for(int i = 0; i < (int)BANNER.MAX; i++)
        {
            id[i] = "unexpected_platform";
        }
#endif
        // バナー設定
        // Create a 320x50 banner at the top of the screen.
        banner[(int)BANNER.TOP]    = new BannerView(id[(int)BANNER.TOP], AdSize.Banner, AdPosition.Top);
        banner[(int)BANNER.BOTTOM] = new BannerView(id[(int)BANNER.BOTTOM], AdSize.Banner, AdPosition.Bottom);

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();

        // 広告の読み込みと非表示化
        // Load the banner with the request.
        for (int i = 0; i < (int)BANNER.MAX; i++)
        {
            banner[i].LoadAd(request);
            banner[i].Hide();
        }
    }
    public void RequestNativeExpress()
    {
        request  = new AdRequest.Builder().Build();
    }

    public void RequestInterstitial()
    {
#if UNITY_ANDROID
        string adUnitId = Android_Interstitial;
#elif UNITY_IPHONE
        string adUnitId = ios_Interstitial;
#else
        string adUnitId = "unexpected_platform";
#endif

        if (is_close_interstitial == true)
        {
            interstitial.Destroy();
        }

        // Initialize an InterstitialAd.
        interstitial = new InterstitialAd(adUnitId);
        // Create an empty ad request.
        request = new AdRequest.Builder().Build();
        // Load the interstitial with the request.
        interstitial.LoadAd(request);

        interstitial.OnAdClosed += HandleAdClosed;
        interstitial.OnAdFailedToLoad += HandleAdReLoad;

        is_close_interstitial = false;
    }

    // インタースティシャル広告を閉じた時に走る
    void HandleAdClosed(object sender, System.EventArgs e)
    {
        is_close_interstitial = true;

        RequestInterstitial();
    }
    // 広告のロードに失敗したときに走る
    void HandleAdReLoad(object sender, System.EventArgs e)
    {
        is_close_interstitial = true;

        StartCoroutine(_waitConnect());
    }

    // 次のロードまで30秒待つ
    IEnumerator _waitConnect()
    {
        while (true)
        {
            yield return new WaitForSeconds(30.0f);

            // ネットに接続できるときだけリロード
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                RequestInterstitial();
                break;
            }
        }
    }
    // 次のロードまで30秒待つ
    IEnumerator _waitConnectRewardVideo()
    {
        while (true)
        {
            yield return new WaitForSeconds(30.0f);

            // ネットに接続できるときだけリロード
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                RequestRewardBasedVideo();
                break;
            }
        }
    }
    /// <summary>
    /// インタースティシャル表示
    /// </summary>
    public void ShowInterstitial()
    {
        if (interstitial.IsLoaded())
        {
            interstitial.Show();
        }
    }
    /// <summary>
    /// バナーの表示,非表示
    /// </summary>
    public void ShowBanner(BANNER inBanner, bool inIsShow)
    {
        if(inIsShow)
        {
            banner[(int)inBanner].Show();
            beforeBanner[(int)inBanner] = true;
        }
        else
        {
            banner[(int)inBanner].Hide();
            beforeBanner[(int)inBanner] = false;
        }
    }
#if USE_NEND
    /// <summary>
    /// インタースティシャル表示
    /// </summary>
    public void ShowNativeAd(bool inIsShow)
    {
        if (inIsShow)
        {
            myNendNative.Show(true);
        }
        else
        {
            myNendNative.Show(false);
        }
    }
#endif
    /// <summary>
    /// 動画表示
    /// </summary>
    public void ShowVideo()
    {
        isMovieReward = false;
        rewardBasedVideo.Show();
    }

    /// <summary>
    /// 広告全体の表示非表示を切り替える
    /// </summary>
    public void ShowTitleFormat()
    {
#if USE_NEND
        ShowNativeAd(false);
#endif
        for (int i = 0; i < (int)BANNER.MAX; i++)
        {
            if(i == (int)BANNER.TOP)
            {
                ShowBanner((BANNER)i, true);
            }
            else
            {
                ShowBanner((BANNER)i, false);
            }
        }
    }
    /// <summary>
    /// 広告全体の表示非表示を切り替える
    /// </summary>
    public void ShowMusicSelectFormat()
    {
#if USE_NEND
        ShowNativeAd(false);
#endif
        for (int i = 0; i < (int)BANNER.MAX; i++)
        {
            if (i == (int)BANNER.TOP)
            {
                ShowBanner((BANNER)i, true);
            }
            else
            {
                ShowBanner((BANNER)i, false);
            }
        }
    }
    /// <summary>
    /// 広告全体の表示非表示を切り替える
    /// </summary>
    public void ShowLoadFormat()
    {
        // フェード時間分待機
        StartCoroutine("ShowLoadFormatAfterWait", LibBridgeInfo.NORMAL_FADEOUT_TIME);
    }
    IEnumerator ShowLoadFormatAfterWait(float inWaitTime)
    {
        yield return new WaitForSeconds(inWaitTime);
#if USE_NEND
        ShowNativeAd(true);
#endif
        for (int i = 0; i < (int)BANNER.MAX; i++)
        {
            ShowBanner((BANNER)i, true);
        }
    }
    /// <summary>
    /// 広告全体の表示非表示を切り替える
    /// </summary>
    public void ShowBattleFormat()
    {
#if USE_NEND
        ShowNativeAd(true);
#endif
        for (int i = 0; i < (int)BANNER.MAX; i++)
        {
            if (i == (int)BANNER.TOP)
            {
                ShowBanner((BANNER)i, true);
            }
            else
            {
                ShowBanner((BANNER)i, false);
            }
        }
    }
    /// <summary>
    /// 広告全体の表示非表示を切り替える
    /// </summary>
    public void ShowResultFormat()
    {
#if USE_NEND
        ShowNativeAd(true);
#endif
        for (int i = 0; i < (int)BANNER.MAX; i++)
        {
            if (i == (int)BANNER.BOTTOM)
            {
                ShowBanner((BANNER)i, true);
            }
            else
            {
                ShowBanner((BANNER)i, false);
            }
        }
        ShowInterstitial();
    }
    /// <summary>
    /// 全体を非表示
    /// </summary>
    public void AllHide()
    {
        // この関数でのみ
        // Show～関数での非表示を行わない
#if USE_NEND
        ShowNativeAd(true);
#endif
        for (int i = 0; i < (int)BANNER.MAX; i++)
        {
            banner[i].Hide();
        }
    }
    /// <summary>
    /// 全非表示前の広告に戻す
    /// </summary>
    public void RevertFromAllHide()
    {
        for (int i = 0; i < (int)BANNER.MAX; i++)
        {
            if (beforeBanner[i])
            {
                ShowBanner((BANNER)i, true);
            }
            else
            {
                ShowBanner((BANNER)i, false);
            }
        }
    }

    /// <summary>
    /// 動画広告視聴完了時処理
    /// </summary>
    void OnAdRewarded(object inObject, Reward inReward)
    {
        Debug.Log("AdRewarded!!!");
        if (inReward != null)
        {
            isMovieReward = true;
            Debug.Log("AdRewardedRewardType["   + inReward.Type + "]");
            Debug.Log("AdRewardedRewardAmount[" + inReward.Amount.ToString() + "]");
        }
    }
    /// <summary>
    /// 動画広告を閉じた際の処理
    /// </summary>
    void OnAdClosed(object inObject, System.EventArgs inArgs)
    {
        Debug.Log("AdClosed");
        if(isMovieReward)
        {
            CommonGameData.Inst.OnCompleteReward();
            RewardAction rewardAction = FindObjectOfType<RewardAction>();
            if (rewardAction != null)
            {
                rewardAction.Initialize();
            }
        }
        StartCoroutine("_waitConnectRewardVideo");
    }
}
#endif