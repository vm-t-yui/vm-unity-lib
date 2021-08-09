/******************************************************************************/
/*!    \brief  FPSとかいろいろ表示.
*******************************************************************************/

using UnityEngine;
using System.Text;

namespace VMUnityLib
{
    [ExecuteInEditMode()]
    public sealed class AllocationStats : MonoBehaviour
    {

        public bool show = true;
        public bool showFPS = false;
        public bool showInEditor = false;
        public void Start()
        {
            useGUILayout = false;
        }

        const int fpsCountNum = 5;
        int fpsCount = 0;
        float fps = 0;
        float fpsSum = 0;

        // Use this for initialization
        public void OnGUI()
        {
            if (!show || (!Application.isPlaying && !showInEditor))
            {
                return;
            }

            int collCount = System.GC.CollectionCount(0);

            if (lastCollectNum != collCount)
            {
                lastCollectNum = collCount;
                delta = Time.realtimeSinceStartup - lastCollect;
                lastCollect = Time.realtimeSinceStartup;
                lastDeltaTime = Time.deltaTime;
                collectAlloc = allocMem;
            }

            allocMem = (int)System.GC.GetTotalMemory(false);

            peakAlloc = allocMem > peakAlloc ? allocMem : peakAlloc;

            if (Time.realtimeSinceStartup - lastAllocSet > 0.3F)
            {
                int diff = allocMem - lastAllocMemory;
                lastAllocMemory = allocMem;
                lastAllocSet = Time.realtimeSinceStartup;

                if (diff >= 0)
                {
                    allocRate = diff;
                }
            }

            StringBuilder text = new StringBuilder();

            text.Append("Currently allocated    ");
            text.Append((allocMem / 1000000F).ToString("0"));
            text.Append("mb\n");

            text.Append("Peak allocated    ");
            text.Append((peakAlloc / 1000000F).ToString("0"));
            text.Append("mb (last collect ");
            text.Append((collectAlloc / 1000000F).ToString("0"));
            text.Append(" mb)\n");


            text.Append("Allocation rate        ");
            text.Append((allocRate / 1000000F).ToString("0.0"));
            text.Append("mb\n");

            text.Append("Collection frequency        ");
            text.Append(delta.ToString("0.00"));
            text.Append("s\n");

            text.Append("Last collect delta        ");
            text.Append(lastDeltaTime.ToString("0.000"));
            text.Append("s (");
            text.Append((fps).ToString("0.0"));

            text.Append(" fps)\n");

            text.Append("SystemMemorySize        ");
            text.Append(SystemInfo.systemMemorySize.ToString());
            text.Append("mb");

            text.Append("\nApplication.targetFrameRate:" + Application.targetFrameRate + " " + QualitySettings.GetQualityLevel() + " " + QualitySettings.vSyncCount);

            /*
            if (showFPS) {
                text.Append ("\n"+(1F/Time.deltaTime).ToString ("0.0")+" fps");
            }
            if (Event.current.type == EventType.Layout)
            {
                GUILayout.BeginVertical ("box");
                GUILayout.Label (text.ToString());
                GUILayout.EndVertical ();
            }
            */
            float boxW = 450;
            float boxH = 135 + (showFPS ? 32 : 0);
            GUI.Box(GUIHelper.GetScaledRectWithoutSpace((LibBridgeInfo.FIXED_SCREEN_WI - boxW - 5), 5, boxW, boxH), "");
            GUI.Label(GUIHelper.GetScaledRectWithoutSpace((LibBridgeInfo.FIXED_SCREEN_WI - boxW + 10), 5, 1000, 600), text.ToString());
            /*GUI.Label (new Rect (5,5,1000,200),
                "Currently allocated            "+(allocMem/1000000F).ToString ("0")+"mb\n"+
                "Peak allocated                "+(peakAlloc/1000000F).ToString ("0")+"mb "+
                ("(last    collect"+(collectAlloc/1000000F).ToString ("0")+" mb)" : "")+"\n"+
                "Allocation rate                "+(allocRate/1000000F).ToString ("0.0")+"mb\n"+
                "Collection space            "+delta.ToString ("0.00")+"s\n"+
                "Last collect delta            "+lastDeltaTime.ToString ("0.000") + " ("+(1F/lastDeltaTime).ToString ("0.0")+")");*/
        }

        float lastCollect = 0;
        float lastCollectNum = 0;
        float delta = 0;
        float lastDeltaTime = 0;
        int allocRate = 0;
        int lastAllocMemory = 0;
        float lastAllocSet = -9999;
        int allocMem = 0;
        int collectAlloc = 0;
        int peakAlloc = 0;
        void Update()
        {
            ++fpsCount;
            if (fpsCount <= fpsCountNum)
            {
                fpsSum += Time.deltaTime;
            }
            else
            {
                fps = 1F / (fpsSum / fpsCountNum);
                fpsCount = 0;
                fpsSum = 0;
            }
        }
    }

}