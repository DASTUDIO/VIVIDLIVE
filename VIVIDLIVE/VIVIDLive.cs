using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using EWindows;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;

[DisallowMultipleComponent]
public class VIVIDLive : MonoBehaviour , IVIVIDPlugin
{
    // ON PLUGIN SCAN
    public static void OnLoad()
    {
        if(!EInspectorCustom.IgnoredComponentsAtAutoScan.Contains(typeof(VIVIDLive)))
            EInspectorCustom.IgnoredComponentsAtAutoScan.Add(typeof(VIVIDLive));
        
        if(!EInspectorCustom.IgnoredComponentsAtAutoScan.Contains(typeof(FaceSetter)))
            EInspectorCustom.IgnoredComponentsAtAutoScan.Add(typeof(FaceSetter));
        
        EInspectorCustom.CustomInspector<VIVIDLive>((i, c) =>
        {
            VIVIDLive vmc = (VIVIDLive)c;
            EInspectorCustom.AddTitleField(i, vmc.GetType().Name, c);

            EInspectorCustom.AddBoolField(i, vmc.START ? "STOP" : "START", vmc.START, b => { vmc.START = b; });
            EInspectorCustom.AddTextField(i, "UDP_PORT(Custom Mode)", vmc.PORT, s => { vmc.PORT = int.Parse(s); });

            EInspectorCustom.AddBoolField(i, "Orthographic", Camera.main.orthographic,
                b => { Camera.main.orthographic = b; });
            EInspectorCustom.AddTextField(i, "Orthographic Size", Camera.main.orthographicSize,
                str => { Camera.main.orthographicSize = float.Parse(str); });

            EInspectorCustom.AddConfirmBoolField(i, "REMOVE", "Remove this component?", false, b =>
            {
                Destroy(c);
                i.Refresh();
            }, false, "REMOVE");
        });

        EInspectorCustom.CustomInspector<FaceSetter>((j, c) =>
        {
            FaceSetter fs = (FaceSetter)c;
            string[] keys = new string[] { "A", "I", "U", "E", "O", "Blink" };
            if (fs.TryGetComponent(out VBlendShapes vbs))
            {
                foreach (var item in keys)
                {
                    j.visualize(EMemory.instance.FieldDropdown, dropdown_field =>
                    {
                        dropdown_field.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text =
                            ETranslation.Translate(item);

                        UnityEngine.UI.Dropdown dropdown = dropdown_field.transform.GetChild(1)
                            .GetComponent<UnityEngine.UI.Dropdown>();

                        int i = 0, res = 0;
                        foreach (var key in vbs.GetAllKeys())
                        {
                            dropdown.options.Add(new UnityEngine.UI.Dropdown.OptionData() { text = key });
                            if (key == fs.blendShapeKeys[item]) res = i;
                            i++;
                        }

                        dropdown.value = res;
                        dropdown.onValueChanged.AddListener((index) =>
                        {
                            fs.blendShapeKeys[item] = dropdown.options[index].text;
                        });
                    });
                }
            }
        });
        
    }

    VAvatar vavatar;

    void OnDestroy()
    {
        Camera.main.orthographic = false;
        
        try { server.Close(); } catch { }
        if (_start)
        {
            StopNodeProcess();
            try { server.Close(); } catch { }

            Destroy(gameObject.GetComponent<VUDPInvoker>());
            Destroy(gameObject.GetComponent<PoseSetter>());
            Destroy(gameObject.GetComponent<FaceSetter>());
        }
    }

    void Start()
    {
        if (!TryGetComponent(out vavatar))
        {
            EMsgbox.Msg("Warning", "This component needs the 'VAvatar' component", () => { }, () => { }); Destroy(this);
            ESelection.openedInspectedObjs[gameObject].GetComponentInChildren<EInspector>().Refresh();
            return;
        }
        vavatar.HeadLookAt = false;
        vavatar.ChestLookAt = false;
        vavatar.LeftHandIK = false;
        vavatar.RightHandIK = false;
        vavatar.LeftFootIK = false;
        vavatar.RightFootIK = false;
    }

    PoseSetter poseSetter;
    FaceSetter faceSetter;
    VUDPServer server;
    bool _start;

    FaceSetter kalidoSetter;

    [NoRecord] public int PORT { get; set; } = 8089;

    [NoRecord]
    public bool START
    {
        get { return _start; }
        set
        {
            if (value)
            {
                if (TryGetComponent(out VExpression ve))     // prepare express
                {
                    if (ve.Blink) { ve.Blink = false; ve.handler.ImmediatelySetValue(VRM.BlendShapeKey.CreateFromPreset(VRM.BlendShapePreset.Blink), 0); }
                    if (ve.Singing) { ve.Singing = false; ve.handler.ImmediatelySetValue(ve.GetBlendShapeKeyByName(ve.key), 0); }
                }

                try
                {
                    gameObject.AddComponent<VUDPInvoker>();
                    poseSetter = gameObject.AddComponent<PoseSetter>();
                    faceSetter = gameObject.AddComponent<FaceSetter>();

                    server = new VUDPServer(PORT, (endPoint, bytes) =>
                    {
                        CaptureData data = JsonConvert.DeserializeObject<CaptureData>(Encoding.UTF8.GetString(bytes));

                        if (data.type == 0) { poseSetter.SetPose(data.pose); faceSetter.SetFace(data.face); return; }
                        if (data.type == 1) { faceSetter.SetFace(data.face); return; }
                        if (data.type == 2) { poseSetter.SetPose(data.pose); return; }
                        if (data.type == 3) { Z.Zarch.code = data.zcode.Replace(@"@this", EUtil.F2S("$", faceSetter.name)); return; }
                    });

                    if (System.IO.File.Exists(nodePath)) nodeProcess = Process.Start(nodePath);

                    EMsgbox.Msg("VIVID Live", "VIVID Live start successful.", () => { }, () => { });


                }
                catch (System.Exception e)
                {
                    StopNodeProcess();
                    try { server.Close(); } catch { }

                    Destroy(gameObject.GetComponent<VUDPInvoker>());
                    Destroy(gameObject.GetComponent<PoseSetter>());
                    Destroy(gameObject.GetComponent<FaceSetter>());

                    Z.Zarch.Log(e.Message);
                }
            }
            else
            {
                StopNodeProcess();
                try { server.Close(); } catch { }

                Destroy(gameObject.GetComponent<VUDPInvoker>());
                Destroy(gameObject.GetComponent<PoseSetter>());
                Destroy(gameObject.GetComponent<FaceSetter>());

                EMsgbox.Msg("VIVID Live", "VIVID Live has been stopped.", () => { }, () => { });
            }

            _start = value;

            if (ESelection.openedInspectedObjs.ContainsKey(gameObject))
                ESelection.openedInspectedObjs[gameObject].GetComponent<EInspector>().Refresh();

        }
    }

    #region Node Server

    public static string nodePath
    {
        get
        {
            return Path.Combine(VAPI.PluginPath,"Live/vividcapture.exe");
// #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
//             return Application.streamingAssetsPath + "/VLive/x86_64/vividcapture.exe";
// #elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
//             return Application.streamingAssetsPath + "/VLive/vividcapture";
// #else
//             return "";
// #endif
         }
    }

    Process nodeProcess;

    void StopNodeProcess()
    {
        try
        {
            Process[] workers = Process.GetProcessesByName("vividcapture");

            foreach (Process worker in workers)
            {
                worker.Kill();
                worker.WaitForExit();
                worker.Dispose();
            }
        }
        catch { }

        try { nodeProcess.Close(); } catch { }
    }

    #endregion

}

public struct CaptureData
{
    public BlazePose[] pose;
    public KalidoFace face;
    public string zcode;
    public int type;
}