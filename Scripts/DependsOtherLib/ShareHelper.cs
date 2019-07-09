/******************************************************************************/
/*!    \brief  シェア機能を統合するユーティリティ.
*******************************************************************************/

using UnityEngine;
using System.Collections;
using System.IO;
using VMUnityLib;
using SWorker;

public sealed class ShareHelper : SingletonMonoBehaviour<ShareHelper> 
{
    const string fileName = "screenshot.png";

    bool capturing = false;
    string captureFilePath;

    /// <summary>
    /// 開始時.
    /// </summary>
    void Start()
    {
        captureFilePath = Application.persistentDataPath + "/" + fileName;
    }

    /// <summary>
    /// スクリーンショット保存.
    /// </summary>
    public void CaptureScreenShot()
    {
        StartCoroutine(CaptureScreenShotCoroutine());
    }
    IEnumerator CaptureScreenShotCoroutine()
    {
        capturing = true;
#if ENABLE_ADMOB
        // キャプチャのために広告を非表示.
        AdMobManager.Inst.AllHide();
#endif
#if USE_NEND
        bool isShowTopBanner = NendAdController.Inst.IsShowTopBanner;
        bool isShowBottomBanner = NendAdController.Inst.IsShowBottomBanner;
        if(isShowTopBanner)
            NendAdController.Inst.ShowTopBanner(false);
        if(isShowBottomBanner)
            NendAdController.Inst.ShowBottomBanner(false);
#endif
        // レイアウト設定のために1フレーム待つ
        yield return LibBridgeInfo.WaitForEndOfFrame;

#if UNITY_EDITOR && UNITY_2017_OR_NEWER
        ScreenCapture.CaptureScreenshot(captureFilePath);
#elif UNITY_EDITOR
        ScreenCapture.CaptureScreenshot(captureFilePath);
#else
        ScreenCapture.CaptureScreenshot(fileName);
#endif

        // キャプチャを保存する処理として１フレーム待つ
        yield return LibBridgeInfo.WaitForEndOfFrame;

        // インジケーター表示
#if !DISABLE_SHARE_HELP
    #if UNITY_IPHONE
            Handheld.SetActivityIndicatorStyle(UnityEngine.iOS.ActivityIndicatorStyle.White);
    #elif UNITY_ANDROID
            Handheld.SetActivityIndicatorStyle(AndroidActivityIndicatorStyle.Small);
    #endif
        Handheld.StartActivityIndicator();
#endif
        // スクリーンショットが保存されるまで待機
        //long filesize = 0;
        //while (filesize == 0)
        //{
        //    yield return null;

        //    //ファイルのサイズを取得
        //    System.IO.FileInfo fi = new System.IO.FileInfo(captureFilePath);
        //    if (fi != null)
        //    {
        //        filesize = fi.Length;
        //    }
        //}
        bool isFileComplete = false;
        while (!isFileComplete)
        {
            Debug.Log("Saving !! ");
            yield return LibBridgeInfo.WaitForEndOfFrame;

            bool isFileExists = File.Exists(captureFilePath);
            bool isFileLocked = false; ;
            FileStream stream = null;
            try
            {
                stream = new FileStream(captureFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch
            {
                Debug.Log("ファイルがロックされているか又は開けない.");
                isFileLocked = true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            isFileComplete = (isFileExists == true && isFileLocked != true) ? true : false;
        }

#if !DISABLE_SHARE_HELP
        // インジケーター非表示
        Handheld.StopActivityIndicator();
#endif
#if ENABLE_ADMOB
        AdMobManager.Inst.RevertFromAllHide();
#endif
#if USE_NEND
        if(isShowTopBanner)
            NendAdController.Inst.ShowTopBanner(isShowTopBanner);
        if(isShowBottomBanner)
            NendAdController.Inst.ShowBottomBanner(isShowBottomBanner);
#endif
    }

    /// <summary>
    /// ツイッターでシェア.
    /// </summary>
    public void ShareOnTwitter(string text, string url)
    {
#if !DISABLE_SHARE_HELP
        StartCoroutine(ShareOnTwitterCoroutine(text, url));
#endif
    }
    IEnumerator ShareOnTwitterCoroutine(string text, string url)
    {
        if (capturing)
            yield return LibBridgeInfo.WaitForEndOfFrame;

        SocialWorker.PostTwitter(text, url, captureFilePath);
    }

    /// <summary>
    /// フェイスブックでシェア.
    /// </summary>
    public void ShareOnFaceBook()
    {
#if !DISABLE_SHARE_HELP
        StartCoroutine(ShareOnFaceBookCoroutine());
#endif
    }
    IEnumerator ShareOnFaceBookCoroutine()
    {
        if (capturing)
            yield return LibBridgeInfo.WaitForEndOfFrame;

        SocialWorker.PostFacebook(captureFilePath);
    }

    /// <summary>
    /// LINEでシェア.
    /// </summary>
    public void ShareOnLine(string text)
    {
#if !DISABLE_SHARE_HELP
        SocialWorker.PostLine(text, string.Empty);
#endif
    }

    /// <summary>
    /// LINEでシェア.
    /// </summary>
    public void ShareOnLine()
    {
#if !DISABLE_SHARE_HELP
        StartCoroutine(ShareOnLineCoroutine());
#endif
    }
    IEnumerator ShareOnLineCoroutine()
    {
        if (capturing)
            yield return LibBridgeInfo.WaitForEndOfFrame;

        SocialWorker.PostLine(string.Empty, captureFilePath);
    }

    /// <summary>
    /// インスタグラムでシェア.
    /// </summary>
    public void ShareOnInstagram()
    {
#if !DISABLE_SHARE_HELP
        StartCoroutine(ShareOnInstagramCoroutine());
#endif
    }
    IEnumerator ShareOnInstagramCoroutine()
    {
        if (capturing)
            yield return LibBridgeInfo.WaitForEndOfFrame;

        SocialWorker.PostInstagram(captureFilePath);
    }

    /// <summary>
    /// 投稿可能な全てのアプリを選択式に表示.
    /// </summary>
    public void ShareOnChooser(string inText)
    {
#if !DISABLE_SHARE_HELP
        StartCoroutine(ShareOnChooserCoroutine(inText));
#endif
    }
    IEnumerator ShareOnChooserCoroutine(string inText)
    {
        if (capturing)
            yield return LibBridgeInfo.WaitForEndOfFrame;

        SocialWorker.CreateChooser(inText, captureFilePath);
    }
}