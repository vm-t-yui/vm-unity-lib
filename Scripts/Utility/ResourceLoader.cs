/******************************************************************************/
/*!    \brief  リソースのロードを行う.
*******************************************************************************/

using UnityEngine;
namespace VMUnityLib
{
    public delegate void EndLoadResourcesCallback(string path, Object loadedObj);
public delegate void EndLoadAssetBundleCallback(string path, AssetBundle loadedObj);

public sealed class ResourceLoader : SingletonMonoBehaviour<ResourceLoader> 
{

    //    // デバッグ用の再マウント種別.
    //    public enum RemountKind
    //    {
    //        Web,        // ウェブ上.
    //        LocalData   // ローカルデータ上.
    //    }

    //    const string LANG_PATH_JP             = "/jpn";                   // 言語パス：日本語.
    //    const string REGION_PATH_JP           = "/JP";                    // リージョンパス：日本.

    //    const string PLATFORM_PATH_WEB        = "/web";                   // プラットフォームパス：Web.

    //#if UNITY_IOS
    //    const string PLATFORM_PATH_IOS        = "/iOS";                   // プラットフォームパス：iOS.
    //#elif UNITY_ANDROID && !UNITY_EDITOR
    //    const string PLATFORM_PATH_ANDROID    = "/android";               // プラットフォームパス：Android.
    //#endif

    //    //static string RESOURCES_PATH = "file://" + Application.dataPath + "/Resources";        // 埋め込みResourcesのパス.

    //    const string ASSETBUNDLE_GROBALPATH = "http://" + LibBridgeInfo.GAME_SERVER_URL;         // アセットバンドルのオンラインファイル時の先頭パス.

    //    static string ASSETBUNDLE_DEF_LOCALPATH; // アセットバンドルのローカルファイル時の先頭パス（デフォルト）.
    //    static string ASSETBUNDLE_LOCALPATH;     // アセットバンドルのローカルファイル時の先頭パス.

    //    string PLATFORM;            // プラットフォームパス.
    //    string LANG;                // 言語パス.
    //    string REGION;              // リージョンパス.

    //    //string resourcesHeadPath;   // リソースの先頭パス.
    //    string assetBundleHeadPath; // アセットバンドルの先頭パス.

    //    /// <summary>
    //    /// Awake.
    //    /// </summary>
    //    public void Awake()
    //    {
    //        // Clear Cache
    //        Caching.CleanCache();

    //        if (this != Inst)
    //        {
    //            Destroy(this);
    //            return;
    //        }

    //        DontDestroyOnLoad(this.gameObject);
    //        MountPath();
    //    }

    //    /*
    //    /// <summary>
    //    /// Resourcesのロードを行う　TODO:テスト.
    //    /// </summary>
    //    public void LoadResources(string path, EndLoadResourcesCallback callback)
    //    {
    //        StartCoroutine(LoadResourcesInternal(path, callback));
    //    }
    //    */

    //    /// <summary>
    //    /// アセットバンドルのロードを行う.
    //    /// </summary>
    //    public void LoadAssetBundle(string path, EndLoadAssetBundleCallback callback)
    //    {
    //        StartCoroutine(LoadAssetBundleInternal(path, callback));
    //    }

    //    /// <summary>
    //    /// デバッグ用：アセットバンドルパス再マウント処理.
    //    /// </summary>
    //    /// <param name="kind">再マウントの種別.</param>
    //    public void DebugAssetBundlePathRemount(RemountKind kind)
    //    {
    //        switch(kind)
    //        {
    //            case RemountKind.LocalData:
    //                assetBundleHeadPath = ASSETBUNDLE_LOCALPATH;
    //                break;

    //            case RemountKind.Web:
    //                assetBundleHeadPath = ASSETBUNDLE_GROBALPATH;
    //                break;
    //        }
    //    }

    //    /*
    //    /// <summary>
    //    /// リソースファイルロード内部処理.
    //    /// </summary>
    //    IEnumerator LoadResourcesInternal(string path, EndLoadResourcesCallback callback)
    //    {
    //        Logger.Log("Load called:" + path);
    //        ResourceRequest resReq = Resources.LoadAsync(resourcesHeadPath + PLATFORM + REGION + LANG + path);

    //        while (resReq.isDone == false)
    //        {
    //            yield return null;
    //        }

    //        if (resReq.asset == null)
    //        {
    //            Logger.Error("ファイルの読み込みに失敗しました。");
    //        }
    //        else
    //        {
    //            callback(path, resReq.asset);
    //        }
    //    }
    //    */

    //    /// <summary>
    //    /// アセットバンドルロード内部処理.
    //    /// </summary>
    //    IEnumerator LoadAssetBundleInternal(string path, EndLoadAssetBundleCallback callback)
    //    {
    //        string loadPath = assetBundleHeadPath + PLATFORM + REGION + LANG + path;
    //        Logger.Log("Load called:" + loadPath);

    //        // キャッシュシステムの準備が完了するのを待ちます
    //        while (!Caching.ready || !Caching.enabled)
    //        {
    //            yield return null;
    //        }
    //        Logger.Log("Caching ready");

    //        // TODO:バージョンを管理する.
    //        int version = 0;

    //        // 同じバージョンが存在する場合はアセットバンドルをキャッシュからロードするか、またはダウンロードしてキャッシュに格納します。
    //        using (WWW www = WWW.LoadFromCacheOrDownload(loadPath, version))
    //        {
    //            Logger.Log("Load start");

    //            yield return www;
    //            if (www.error != null)
    //            {
    //                Logger.Error("WWWダウンロードにエラーがありました:" + www.error);
    //            }
    //            else
    //            {
    //                AssetBundle bundle = www.assetBundle;
    //                if (bundle == null)
    //                {
    //                    Logger.Error("ファイルの読み込みに失敗しました。");
    //                }
    //                else
    //                {
    //                    callback(path, bundle);
    //                    Logger.Log("callback end");

    //                    // メモリ節約のため圧縮されたアセットバンドルのコンテンツをアンロード
    //                    bundle.Unload(false);
    //                    Logger.Log("load completed.");
    //                }
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// 各リソースのパスのマウントを行う.
    //    /// </summary>
    //    void MountPath()
    //    {
    //        ASSETBUNDLE_DEF_LOCALPATH = "file:///" + Application.streamingAssetsPath; // アセットバンドルのローカルファイル時の先頭パス（デフォルト）.

    //    // アセットバンドルのローカルファイル時の先頭パス.
    //#if !UNITY_EDITOR
    //    #if UNITY_IOS
    //        // TODO:テスト
    //        ASSETBUNDLE_LOCALPATH = "file://" + Application.dataPath + ""/Raw";
    //    #elif UNITY_ANDROID
    //        // FIXME:動かない
    //        ASSETBUNDLE_LOCALPATH = "jar:file://" + Application.dataPath + "!/assets/";
    //    #else
    //        ASSETBUNDLE_LOCALPATH = ASSETBUNDLE_DEF_LOCALPATH;
    //    #endif
    //#else
    //        ASSETBUNDLE_LOCALPATH = ASSETBUNDLE_DEF_LOCALPATH;     
    //#endif

    //        // プラットフォームパス.
    //#if !UNITY_EDITOR
    //    #if UNITY_IPHONE
    //        PLATFORM = PLATFORM_PATH_IOS;    
    //    #elif UNITY_ANDROID
    //        PLATFORM = PLATFORM_PATH_ANDROID;
    //    #else
    //        PLATFORM = PLATFORM_PATH_WEB;    
    //    #endif
    //#else
    //        PLATFORM = PLATFORM_PATH_WEB;
    //#endif

    //        // 言語パス.
    //        // TODO:別の場所に保存して、動的切り替えが出来るように.
    //        LANG = LANG_PATH_JP;        

    //        // リージョンパス.
    //        // TODO:別の場所に保存して、動的切り替えが出来るように.
    //        REGION = REGION_PATH_JP;

    //        // リソースの先頭パス.
    //        //resourcesHeadPath = RESOURCES_PATH;   

    //        // アセットバンドルの先頭パス.
    //        assetBundleHeadPath = ASSETBUNDLE_LOCALPATH;
    //        //assetBundleHeadPath = ASSETBUNDLE_GROBALPATH;
    //    }
    }
}