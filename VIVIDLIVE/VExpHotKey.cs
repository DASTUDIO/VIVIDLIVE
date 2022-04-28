using System;
using UnityEngine;
using EWindows;
using System.Collections.Generic;
using VRM;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class VExpHotKey : MonoBehaviour, IVIVIDPlugin
{
    // ON PLUGIN SCAN
    public static void OnLoad()
    {
         EInspectorCustom.CustomInspector<VExpHotKey>((j, c) =>
            {
                VExpHotKey veh = (VExpHotKey)c;

                if (veh.isVRM)
                    foreach (var item in veh.codes)
                    {
                        j.visualize(EMemory.instance.FieldDropdown, dropdown_field =>
                        {
                            dropdown_field.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text =
                                item.ToString();

                            UnityEngine.UI.Dropdown dropdown = dropdown_field.transform.GetChild(1)
                                .GetComponent<UnityEngine.UI.Dropdown>();

                            var clips = veh.vrmExp.handler.BlendShapeAvatar.Clips;

                            int i = 0, res = 0;
                            foreach (var key in clips)
                            {
                                dropdown.options.Add(new UnityEngine.UI.Dropdown.OptionData()
                                    { text = key.BlendShapeName });
                                if (key.BlendShapeName == veh.vrmBlendKeys[item].Name) res = i;
                                i++;
                            }

                            dropdown.value = res;
                            dropdown.onValueChanged.AddListener((index) =>
                            {
                                veh.vrmBlendKeys[item] = VRM.BlendShapeKey.CreateFromClip(clips[index]);
                            });
                        });
                    }
                else
                {
                    foreach (var item in veh.codes)
                    {
                        j.visualize(EMemory.instance.FieldDropdown, dropdown_field =>
                        {
                            dropdown_field.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text =
                                item.ToString();

                            UnityEngine.UI.Dropdown dropdown = dropdown_field.transform.GetChild(1)
                                .GetComponent<UnityEngine.UI.Dropdown>();

                            int i = 0, res = 0;
                            foreach (var key in veh.vbs.GetAllKeys())
                            {
                                dropdown.options.Add(new UnityEngine.UI.Dropdown.OptionData() { text = key });
                                if (key == veh.vbsKey[item]) res = i;
                                i++;
                            }

                            dropdown.value = res;
                            dropdown.onValueChanged.AddListener((index) =>
                            {
                                veh.vbsKey[item] = dropdown.options[index].text;
                            });
                        });
                    }
                }
            });
        
    }

    public float Duration { get; set; } = 3f;

    public float Speed { get; set; } = 15f;

    public KeyCode[] codes = new KeyCode[] { KeyCode.F2, KeyCode.F3, KeyCode.F4, KeyCode.F5, KeyCode.F6, KeyCode.F7 };

    public Dictionary<KeyCode, BlendShapeKey> vrmBlendKeys = new Dictionary<KeyCode, BlendShapeKey>();
    public Dictionary<KeyCode, string> vbsKey = new Dictionary<KeyCode, string>();

    BlendShapeKey[] vbks = new BlendShapeKey[]
    {
        BlendShapeKey.CreateFromPreset(BlendShapePreset.Joy),
        BlendShapeKey.CreateFromPreset(BlendShapePreset.Fun),
        BlendShapeKey.CreateFromPreset(BlendShapePreset.Sorrow),
        BlendShapeKey.CreateFromPreset(BlendShapePreset.Angry),
        BlendShapeKey.CreateFromPreset(BlendShapePreset.Neutral),
        BlendShapeKey.CreateFromPreset(BlendShapePreset.Neutral),
    };

    public VExpression vrmExp;
    public VBlendShapes vbs;
    public bool isVRM;

    BlendShapeKey _vrmTargetKey;
    BlendShapeKey vrmTargetKey { get => _vrmTargetKey; set { vrmExp.handler.ImmediatelySetValue(_vrmTargetKey, 0); _vrmTargetKey = value; } }

    float vrmTargetValue;
    bool isVrmResume;
    
    static void OnKeyDown(KeyCode code)
    {
        if (handler.TryGetComponent(out FaceSetter fs)) if (fs.isPause) return; else fs.Pause(handler.Duration);

        if (handler.isVRM)
        {
            if (handler.vrmExp.Blink) { handler.vrmExp.Blink = false;handler. vrmExp.handler.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink), 0); }
            if (handler.vrmExp.Singing) { handler.vrmExp.Singing = false; handler.vrmExp.handler.ImmediatelySetValue(handler.vrmExp.GetBlendShapeKeyByName(handler.vrmExp.key), 0); }

            handler.vrmTargetKey = handler.vrmBlendKeys[code]; handler.vrmTargetValue = 1;

            handler.StartCoroutine(EMemory.instance.DelayAction(() =>
            {
                handler.vrmTargetKey = handler.vrmBlendKeys[code];
                handler.vrmTargetValue = 0;
                handler.isVrmResume = true;

            }, handler.Duration - Settings.WaitSeconds));
        }
        else
        {
            handler.vbs.set(handler.vbsKey[code], 100); 
            handler.StartCoroutine(EMemory.instance.DelayAction(() => { handler.vbs.set(handler.vbsKey[code], 0); }, handler.Duration));
        }
    }

    static VExpHotKey handler = null;
    
    void Start()
    {
        handler = this;
        
        if (TryGetComponent<VExpression>(out vrmExp)) isVRM = true;
        else if (TryGetComponent<VBlendShapes>(out vbs)) { }
        else { EMsgbox.Msg("VExpHotKey", "'VExpression' or 'VBlenShapes' is required.", () => { }, () => { }); Destroy(this); }

        if (isVRM)
        {
            for (int i = 0; i < codes.Length; i++) vrmBlendKeys[codes[i]] = vbks[i];
            foreach (var clip in vrmExp.handler.BlendShapeAvatar.Clips)
            {
                if (clip.BlendShapeName == "Surprised") vrmBlendKeys[KeyCode.F6] = BlendShapeKey.CreateFromClip(clip);
                if (clip.BlendShapeName == "Extra") vrmBlendKeys[KeyCode.F7] = BlendShapeKey.CreateFromClip(clip);
            }
            _vrmTargetKey = BlendShapeKey.CreateFromPreset(BlendShapePreset.Neutral);
        }
        else
        {
            for (int j = 0; j < codes.Length; j++) { vbsKey[codes[j]] = ""; };
            vbsKey[KeyCode.F2] = " 笑い";
        }

        SetHotKey();
    }

    void OnDestroy()
    {
        ReleaseHotKey();
        if (handler == this) handler = null;
    }

    void Update()
    {
        if (isVRM)
        {
            if (vrmTargetKey.Preset != BlendShapePreset.Neutral)
            {
                float current = Mathf.Lerp(vrmExp.handler.GetValue(vrmTargetKey), vrmTargetValue, (isVrmResume ? Speed * 0.618f : Speed) * Time.deltaTime);
                current = current > 1 ? 1 : current;
                current = current < 0 ? 0 : current;

                vrmExp.handler.ImmediatelySetValue(vrmTargetKey, current);

                if (isVrmResume && vrmExp.handler.GetValue(vrmTargetKey) < 0.1f)
                {
                    vrmExp.handler.ImmediatelySetValue(vrmTargetKey, 0);
                    vrmTargetKey = BlendShapeKey.CreateFromPreset(BlendShapePreset.Neutral);
                    isVrmResume = false;
                }
            }
        }

        // foreach (var item in codes) if (Input.GetKeyDown(item)) OnKeyDown(item);
    }
    
    // #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern IntPtr GetModuleHandle(string lpModuleName);

    private static LowLevelKeyboardProc _proc = HookCallBack;

    static IntPtr HookCallBack(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);

            if (vkCode == 113) 
            {
                OnKeyDown(KeyCode.F2);
            }

            if (vkCode == 114)  
            {
                OnKeyDown(KeyCode.F3);
            }

            if (vkCode == 115)  
            {
                OnKeyDown(KeyCode.F4);
            }

            if (vkCode == 116) 
            {
                OnKeyDown(KeyCode.F5);
            }
            
            if (vkCode == 117) 
            {
                OnKeyDown(KeyCode.F6);
            }
            
            if (vkCode == 118) 
            {
                OnKeyDown(KeyCode.F7);
            }
        }

        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    static IntPtr _hookID = IntPtr.Zero;

    static int WH_KEYBOARD_LL = 13;

    static int WM_KEYDOWN = 0x0100;

    static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }
// #endif
    
    public static void SetHotKey()
    {
// #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if(EMemory.hasHotKey) ReleaseHotKey();
        _hookID = SetHook(_proc);
        EMemory.hasHotKey = true;
// #endif
    }

    public static void ReleaseHotKey()
    {
// #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        UnhookWindowsHookEx(_hookID);
        EMemory.hasHotKey = false;
// #endif
    }
    
}
