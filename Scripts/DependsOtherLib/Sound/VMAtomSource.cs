#if ENABLE_CRI
using UnityEngine;

/// <summary>
/// 自社用のアトムソース（遅延検知への対応）.
/// </summary>
public class VMAtomSource : CriAtomSource
{
    [SerializeField]
    private bool _enableAudioSyncedTimer = false;

    override protected void InternalInitialize()
    {
        CriAtomPlugin.InitializeLibrary();
        player = new CriAtomExPlayer(_enableAudioSyncedTimer);
        source = new CriAtomEx3dSource();
    }
}
#endif