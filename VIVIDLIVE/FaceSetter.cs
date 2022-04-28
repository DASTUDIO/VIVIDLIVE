using UnityEngine;
using System.Collections.Generic;
using EWindows;
using VRM;

[DisallowMultipleComponent]
public class FaceSetter : MonoBehaviour
{
    public float IrisSpeed { get; set; } = 10f;
    public float IrisFactor { get; set; } = 1.5f;

    public float CloseEyeSpeed { get; set; } = 10;
    public float OpenEyeSpeed { get; set; } = 5;
    public float CloseEyeThreshold { get; set; } = 0.7f;

    public float CloseMouthSpeed { get; set; } = 10;
    public float OpenMouthSpeed { get; set; } = 5;


    public Dictionary<string, string> blendShapeKeys = new Dictionary<string, string>();

    bool isVRM = false, hasHead = false, hasBlendShapes = false;
    VRMBlendShapeProxy exp = null;
    VBlendShapes vbs = null;
    Transform lookAtObj = null;

    Dictionary<BlendShapeKey, float> newValues = new Dictionary<BlendShapeKey, float>();
    Dictionary<BlendShapeKey, float> currentValues = new Dictionary<BlendShapeKey, float>();

    BlendShapeKey a = BlendShapeKey.CreateFromPreset(BlendShapePreset.A);
    BlendShapeKey i = BlendShapeKey.CreateFromPreset(BlendShapePreset.I);
    BlendShapeKey u = BlendShapeKey.CreateFromPreset(BlendShapePreset.U);
    BlendShapeKey e = BlendShapeKey.CreateFromPreset(BlendShapePreset.E);
    BlendShapeKey o = BlendShapeKey.CreateFromPreset(BlendShapePreset.O);
    BlendShapeKey blink = BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink);

    void Start()
    {
        Transform headBone = null;

        if (TryGetComponent(out Animator am)) headBone = am.GetBoneTransform(HumanBodyBones.Head); if (headBone != null) hasHead = true;
        if (TryGetComponent<VRMBlendShapeProxy>(out exp)) isVRM = true;
        if (TryGetComponent<VBlendShapes>(out vbs)) hasBlendShapes = true;

        if (isVRM)
        {
            if (hasHead)
            {
                lookAtObj = new GameObject("look at target").transform;
                lookAtObj.parent = headBone;
                lookAtObj.position = Vector3.zero;
                lookAtObj.localPosition = Vector3.forward;
                newIrisPos = lookAtObj.localPosition;
                if (TryGetComponent(out VExpression ev)) ev.lookAt = true;
                if (TryGetComponent(out VRMLookAtHead vla)) vla.Target = lookAtObj;
            }
            newValues[a] = 0;
            newValues[i] = 0;
            newValues[u] = 0;
            newValues[e] = 0;
            newValues[o] = 0;
        }
        else
        {
            if (hasBlendShapes)
            {
                blendShapeKeys["A"] = "あ";
                blendShapeKeys["I"] = "い";
                blendShapeKeys["U"] = "う";
                blendShapeKeys["E"] = "え";
                blendShapeKeys["O"] = "お";
                blendShapeKeys["Blink"] = "なごみ";
            }
        }

    }

    Vector3 newIrisPos; float newBlinkValue;

    public void SetFace(KalidoFace faceData)
    {
        if (isPause) return;

        if (isVRM)
        {
            newValues[a] = faceData.mouth.shape.A;
            newValues[i] = faceData.mouth.shape.I;
            newValues[u] = faceData.mouth.shape.U;
            newValues[e] = faceData.mouth.shape.E;
            newValues[o] = faceData.mouth.shape.O;

            newBlinkValue = 1 - (faceData.eye.l + faceData.eye.r) / 2;

            newBlinkValue = newBlinkValue > CloseEyeThreshold ? 1 : newBlinkValue;

            newIrisPos = new Vector3(-faceData.pupil.x * IrisFactor, faceData.pupil.y * IrisFactor, newIrisPos.z);

            return;
        }

        if (hasBlendShapes)
        {
            if (!string.IsNullOrEmpty(blendShapeKeys["A"])) vbs.set(blendShapeKeys["A"], 100 * faceData.mouth.shape.A);
            if (!string.IsNullOrEmpty(blendShapeKeys["I"])) vbs.set(blendShapeKeys["I"], 100 * faceData.mouth.shape.I);
            if (!string.IsNullOrEmpty(blendShapeKeys["U"])) vbs.set(blendShapeKeys["U"], 100 * faceData.mouth.shape.U);
            if (!string.IsNullOrEmpty(blendShapeKeys["E"])) vbs.set(blendShapeKeys["E"], 100 * faceData.mouth.shape.E);
            if (!string.IsNullOrEmpty(blendShapeKeys["O"])) vbs.set(blendShapeKeys["O"], 100 * faceData.mouth.shape.O);
            if (!string.IsNullOrEmpty(blendShapeKeys["Blink"])) vbs.set(blendShapeKeys["Blink"], 100 * (1 - (faceData.eye.l + faceData.eye.r) / 2));
        }
    }

    public bool isPause = false;
    float pauseTime;
    float pauseDuration;

    public void Pause(float time)
    {
        if (isPause)
        {
            if (pauseTime - pauseDuration < time) { pauseTime = time; pauseDuration = 0; }
        }
        else
        {
            pauseTime = time;
            pauseDuration = 0;
        }

        if (isVRM)
        {
            currentValues[blink] = 0;
            currentValues[a] = 0;
            currentValues[i] = 0;
            currentValues[u] = 0;
            currentValues[e] = 0;
            currentValues[o] = 0;
            exp.SetValues(currentValues);
        }
        else if (hasBlendShapes)
            foreach (var key in blendShapeKeys.Values)
                vbs.set(key, 0);

        isPause = true;
    }

    void Update()
    {
        if (isPause)
        {
            pauseDuration += Time.deltaTime;

            if (pauseDuration > pauseTime)
                isPause = false;

            return;
        }

        if (lookAtObj != null)
            lookAtObj.localPosition = Vector3.Lerp(lookAtObj.localPosition, newIrisPos, IrisSpeed * Time.deltaTime);

        if (isVRM)
        {
            float currentBlink = exp.GetValue(blink);
            currentBlink = Mathf.Lerp(currentBlink, newBlinkValue, (currentBlink < newBlinkValue ? CloseEyeSpeed : OpenEyeSpeed) * Time.deltaTime);
            currentValues[blink] = currentBlink;

            UpdateMouthShape(a);
            UpdateMouthShape(i);
            UpdateMouthShape(u);
            UpdateMouthShape(e);
            UpdateMouthShape(o);

            exp.SetValues(currentValues);
        }
    }

    void UpdateMouthShape(BlendShapeKey key)
    {
        float currentValue = exp.GetValue(key);
        float targetValue = newValues[key];
        currentValue = Mathf.Lerp(currentValue, targetValue, (currentValue < targetValue ? CloseMouthSpeed : OpenMouthSpeed) * Time.deltaTime);
        currentValues[key] = currentValue;
    }
}

public struct Eye { public float l, r; }
public struct Shape { public float A, I, U, E, O; }
public struct Mouth { public float x, y; public Shape shape; }
public struct Head { public float x, y, z, width, height; public Vector3 position, normalized, degrees; }
public struct KalidoFace
{
    public Eye eye;
    public Mouth mouth;
    public Head head;
    public Vector2 pupil;
    public float brow;
}