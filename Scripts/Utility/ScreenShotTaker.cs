using System;
using UnityEditor;
using UnityEngine;

namespace VMUnityLib
{
    /// <summary>
    /// シーン名を定数で管理するクラスを作成するスクリプト
    /// </summary>
    public static class ScreenShotTaker
    {
        const string ITEM_NAME  = "Tools/Game画面解像度でScreenShotを撮る";    // コマンド名
#if  UNITY_EDITOR
        [MenuItem(ITEM_NAME)]
#endif
        public static void TakeScreenShot()
        {
            string desktopDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(desktopDirectoryPath, "screenshot_" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".png"));
        }
    }
}
