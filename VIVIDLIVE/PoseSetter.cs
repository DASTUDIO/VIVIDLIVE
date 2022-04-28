using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PoseSetter : MonoBehaviour
{
    public int BufferSize { get; set; } = 2;

    public float Speed { get; set; } = 1.5f;

    public float Threshold { get; set; } = 0.6f;

    #region Bones

    public enum BoneType : byte
    {
        hips, spine,
        chest, upperChest,

        neck, head,

        shoulderL, upperArmL, lowerArmL, handL,
        shoulderR, upperArmR, lowerArmR, handR,

        upperLegL, lowerLegL, footL,
        upperLegR, lowerLegR, footR,

        thumbL1, middleL1,
        thumbR1, middleR1,

        toesL, toesR
    }

    Dictionary<BoneType, Transform> Bones = new Dictionary<BoneType, Transform>();

    Dictionary<BoneType, Quaternion> InitRotation = new Dictionary<BoneType, Quaternion>(), InverseMap = new Dictionary<BoneType, Quaternion>();

    void MapBones(List<BoneType> boneType, List<HumanBodyBones> humanBone) { for (int i = 0; i < boneType.Count; i++) Bones[boneType[i]] = animator.GetBoneTransform(humanBone[i]); }

    void FillBoneMaps()
    {
        MapBones(new List<BoneType>()
        {
                BoneType.hips, BoneType.spine,
                BoneType.chest, BoneType.upperChest,

                BoneType.neck, BoneType.head,

                BoneType.shoulderL, BoneType.upperArmL, BoneType.lowerArmL, BoneType.handL,
                BoneType.shoulderR, BoneType.upperArmR, BoneType.lowerArmR, BoneType.handR,

                BoneType.upperLegL, BoneType.lowerLegL, BoneType.footL,
                BoneType.upperLegR, BoneType.lowerLegR ,BoneType.footR,

                BoneType.thumbL1, BoneType.middleL1,
                BoneType.thumbR1, BoneType.middleR1,

                BoneType.toesL, BoneType.toesR

            }, new List<HumanBodyBones>()
            {
                HumanBodyBones.Hips, HumanBodyBones.Spine,
                HumanBodyBones.Chest, HumanBodyBones.UpperChest,

                HumanBodyBones.Neck, HumanBodyBones.Head,
                HumanBodyBones.LeftShoulder, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand,
                HumanBodyBones.RightShoulder, HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand,

                HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot,
                HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot,

                HumanBodyBones.LeftThumbProximal, HumanBodyBones.LeftMiddleProximal,
                HumanBodyBones.RightThumbProximal, HumanBodyBones.RightMiddleProximal,

                HumanBodyBones.LeftToes, HumanBodyBones.RightToes
            });

        if (Bones[BoneType.upperChest] == null) Bones[BoneType.upperChest] = animator.GetBoneTransform(HumanBodyBones.Chest);  // fix for blender cats
        if (Bones[BoneType.chest] == null) Bones[BoneType.chest] = animator.GetBoneTransform(HumanBodyBones.UpperChest);       // fix for mmd


        Vector3 forward = TryGetComponent<LibMMD.Unity3D.MMDModel>(out _) ?
            GetNormal(Bones[BoneType.hips].position, Bones[BoneType.upperLegR].position, Bones[BoneType.upperLegL].position) :
            GetNormal(Bones[BoneType.hips].position, Bones[BoneType.upperLegL].position, Bones[BoneType.upperLegR].position);

        InverseMap[BoneType.head] = Quaternion.Inverse(Quaternion.LookRotation(forward));

        InverseMap[BoneType.upperArmL] = GetInverse(Bones[BoneType.upperArmL], Bones[BoneType.lowerArmL], forward);
        InverseMap[BoneType.lowerArmL] = GetInverse(Bones[BoneType.lowerArmL], Bones[BoneType.handL], forward);

        InverseMap[BoneType.upperArmR] = GetInverse(Bones[BoneType.upperArmR], Bones[BoneType.lowerArmR], forward);
        InverseMap[BoneType.lowerArmR] = GetInverse(Bones[BoneType.lowerArmR], Bones[BoneType.handR], forward);

        InverseMap[BoneType.upperLegL] = GetInverse(Bones[BoneType.upperLegL], Bones[BoneType.lowerLegL], forward);
        InverseMap[BoneType.lowerLegL] = GetInverse(Bones[BoneType.lowerLegL], Bones[BoneType.footL], forward);

        InverseMap[BoneType.upperLegR] = GetInverse(Bones[BoneType.upperLegR], Bones[BoneType.lowerLegR], forward);
        InverseMap[BoneType.lowerLegR] = GetInverse(Bones[BoneType.lowerLegR], Bones[BoneType.footR], forward);

        InverseMap[BoneType.chest] = Quaternion.Inverse(Quaternion.LookRotation(forward));
        InverseMap[BoneType.spine] = Quaternion.Inverse(Quaternion.LookRotation(forward));

        InverseMap[BoneType.handL] = Quaternion.identity;
        InverseMap[BoneType.handR] = Quaternion.identity;
        InverseMap[BoneType.footL] = Quaternion.identity;
        InverseMap[BoneType.footR] = Quaternion.identity;

        try
        {
            InverseMap[BoneType.handL] = Quaternion.Inverse(
                Quaternion.LookRotation(Bones[BoneType.handL].position - Bones[BoneType.middleL1].position,
                    GetNormal(
                        Bones[BoneType.handL].position, Bones[BoneType.thumbL1].position,
                        Bones[BoneType.middleL1].position
                        )));
        }
        catch { }

        try
        {
            InverseMap[BoneType.handR] = Quaternion.Inverse(
                Quaternion.LookRotation(Bones[BoneType.handR].position - Bones[BoneType.middleR1].position,
                    GetNormal(
                        Bones[BoneType.handR].position,
                        Bones[BoneType.middleR1].position,
                        Bones[BoneType.thumbR1].position)));
        }
        catch { }

        try
        {
            InverseMap[BoneType.footL] = Quaternion.Inverse(
                Quaternion.LookRotation(Bones[BoneType.footL].position - Bones[BoneType.toesL].position,
                Bones[BoneType.footL].position - Bones[BoneType.lowerLegL].position));
        }
        catch { }

        try
        {
            InverseMap[BoneType.footR] = Quaternion.Inverse(
                Quaternion.LookRotation(Bones[BoneType.footR].position - Bones[BoneType.toesR].position,
                Bones[BoneType.footR].position - Bones[BoneType.lowerLegR].position));
        }
        catch { }

        foreach (var item in Bones.Keys) { try { InitRotation[item] = Bones[item].rotation; } catch { } }
    }

    Animator animator;

    void Start() { animator = GetComponent<Animator>(); FillBoneMaps(); }

    bool isInit = false;

    void Update()
    {
        if (!isInit) return;

        SetHead();

        SetLeftArm(); SetRightArm();

        SetBody();

        SetRightLeg(); SetLeftLeg();

    }

    #endregion

    int FrameCount;

    BlazePose[] Sum = new BlazePose[33];

    BlazePose[] keyFrame = new BlazePose[33];

    public void SetPose(BlazePose[] points)
    {
        for (int i = 0; i < 33; i++) if (FrameCount == 0) Sum[i] = points[i]; else Sum[i] += points[i];

        FrameCount++;

        if (FrameCount >= BufferSize)
        {
            for (int i = 0; i < 33; i++) Sum[i] /= (BufferSize > 0 ? BufferSize : 1);

            Array.Copy(Sum, keyFrame, 33);

            Sum = new BlazePose[33];

            FrameCount = 0;
        }

        if (!isInit) keyFrame = points;

        this.points = keyFrame;

        isInit = true;
    }

    BlazePose[] points;

    Vector3 GetDirection(BlazePose to, BlazePose from) { return to.v3 - from.v3; }
    Vector3 GetAverage(BlazePose p1, BlazePose p2) { return p1.v3 + p2.v3 / 2; }
    Vector3 GetNormal(BlazePose p1, BlazePose p2, BlazePose p3) { return Vector3.Cross(GetDirection(p1, p2), GetDirection(p1, p3)).normalized; }

    Vector3 GetNormal(Vector3 p1, Vector3 p2, Vector3 p3) { return Vector3.Cross(p1 - p2, p1 - p3).normalized; }
    Quaternion GetInverse(Transform from, Transform to, Vector3 up) { return Quaternion.Inverse(Quaternion.LookRotation(from.position - to.position, up)); }

    bool ValidatePoints(params int[] pts) { for (int i = 0; i < pts.Length; i++) if (!points[pts[i]].isVisible(Threshold)) return false; return true; }

    void SetHead() // 0, 2, 5 
    {
        if (!ValidatePoints(10, 9, 8, 7, 6, 3, 0)) return;

        // neck
        Vector3 headForwardDir = Vector3.Cross(GetDirection(points[10], points[3]), GetDirection(points[9], points[6]));
        headForwardDir = new Vector3(headForwardDir.x, 0, headForwardDir.z);

        Vector3 headUp = GetNormal(points[0], points[7], points[8]);
        Bones[BoneType.head].LerpRotateTo(Quaternion.LookRotation(headForwardDir, headUp) * InverseMap[BoneType.head] * InitRotation[BoneType.head], Speed * Time.deltaTime);
    }

    void SetLeftArm() // 12, 14, 16, 18, 20
    {
        if (!ValidatePoints(14, 11, 12)) return;
        Vector3 up12 = GetDirection(points[11], points[12]);
        Vector3 up14 = GetDirection(points[12], points[14]);
        Bones[BoneType.upperArmL].LerpRotateTo(Quaternion.LookRotation(up14, up12) * InverseMap[BoneType.upperArmL] * InitRotation[BoneType.upperArmL], Speed * Time.deltaTime);

        if (!ValidatePoints(16)) return;
        Vector3 up16 = GetDirection(points[14], points[16]);
        Bones[BoneType.lowerArmL].LerpRotateTo(Quaternion.LookRotation(up16, up14) * InverseMap[BoneType.lowerArmL] * InitRotation[BoneType.lowerArmL], Speed * Time.deltaTime);

        if (!ValidatePoints(22, 20, 18)) return;
        Vector3 upHand = GetNormal(points[16], points[18], points[22]);
        Vector3 handDir = points[16].v3 - GetAverage(points[18], points[20]);
        Bones[BoneType.handL].LerpRotateTo(
        Quaternion.AngleAxis(Vector3.Angle(upHand, Vector3.down), handDir)
        * Quaternion.LookRotation(handDir, upHand) * InverseMap[BoneType.handL] * InitRotation[BoneType.handL], Speed * Time.deltaTime);
    }

    void SetRightArm() // 11, 13, 15, 17, 19
    {
        if (!ValidatePoints(13, 11, 12)) return;
        Vector3 up11 = GetDirection(points[12], points[11]);
        Vector3 up13 = GetDirection(points[11], points[13]);
        Bones[BoneType.upperArmR].LerpRotateTo(Quaternion.LookRotation(up13, up11) * InverseMap[BoneType.upperArmR] * InitRotation[BoneType.upperArmR], Speed * Time.deltaTime);

        if (!ValidatePoints(15)) return;
        Vector3 up15 = GetDirection(points[13], points[15]);
        Bones[BoneType.lowerArmR].LerpRotateTo(Quaternion.LookRotation(up15, up13) * InverseMap[BoneType.lowerArmR] * InitRotation[BoneType.lowerArmR], Speed * Time.deltaTime);

        if (!ValidatePoints(21, 17, 19)) return;
        Vector3 upHand = GetNormal(points[15], points[21], points[17]);
        Vector3 handDir = points[15].v3 - GetAverage(points[17], points[19]);
        Bones[BoneType.handR].LerpRotateTo(
        Quaternion.AngleAxis(Vector3.Angle(upHand, Vector3.down), handDir)
        * Quaternion.LookRotation(handDir, upHand) * InitRotation[BoneType.handR] * InverseMap[BoneType.handR], Speed * Time.deltaTime);
    }

    void SetBody() // 11, 12, 23, 24
    {
        if (!ValidatePoints(11, 12, 0)) return;
        Vector3 chestFaceTo = GetNormal(points[0], points[12], points[11]);
        chestFaceTo = new Vector3(chestFaceTo.x, 0, chestFaceTo.z);
        Vector3 chestUp = transform.up;

        if (!ValidatePoints(23, 24, 11, 12))
        {
            Vector3 faceTo = Vector3.Cross(GetDirection(points[11], points[24]), GetDirection(points[12], points[23]));
            faceTo = new Vector3(faceTo.x, 0, faceTo.z);
            Vector3 upDir = GetAverage(points[11], points[12]) - GetAverage(points[23], points[24]);
            Bones[BoneType.spine].LerpRotateTo(Quaternion.LookRotation(faceTo, upDir) * InverseMap[BoneType.spine] * InitRotation[BoneType.spine], Speed * Time.deltaTime);
            chestUp = upDir;
        }

        Bones[BoneType.chest].LerpRotateTo(Quaternion.LookRotation(chestFaceTo, chestUp) * InverseMap[BoneType.chest] * InitRotation[BoneType.chest], Speed * Time.deltaTime);
    }

    void SetLeftLeg() // 24, 26, 28, 30, 32 
    {
        if (!ValidatePoints(26, 24, 23)) return;
        Vector3 up24 = GetDirection(points[23], points[24]);
        Vector3 up26 = GetDirection(points[24], points[26]);
        Bones[BoneType.upperLegL].LerpRotateTo(Quaternion.LookRotation(up26, up24) * InverseMap[BoneType.upperLegL] * InitRotation[BoneType.upperLegL], Speed * Time.deltaTime);

        if (!ValidatePoints(28)) return;
        Vector3 up28 = GetDirection(points[26], points[28]);
        Bones[BoneType.lowerLegL].LerpRotateTo(Quaternion.LookRotation(up28, up26) * InverseMap[BoneType.lowerLegL] * InitRotation[BoneType.lowerLegL], Speed * Time.deltaTime);

        if (!ValidatePoints(30, 32)) return;
        Vector3 footDir = points[28].v3 - GetAverage(points[30], points[32]);
        Vector3 foorUp = GetDirection(points[28], points[32]);
        Bones[BoneType.footL].LerpRotateTo(Quaternion.LookRotation(footDir, foorUp) * InverseMap[BoneType.footL] * InitRotation[BoneType.footL], Speed * Time.deltaTime);
    }

    void SetRightLeg() // 23, 25, 27, 29, 31
    {
        if (!ValidatePoints(25, 24, 23)) return;
        Vector3 up23 = GetDirection(points[24], points[23]);
        Vector3 up25 = GetDirection(points[23], points[25]);
        Bones[BoneType.upperLegR].LerpRotateTo(Quaternion.LookRotation(up25, up23) * InverseMap[BoneType.upperLegR] * InitRotation[BoneType.upperLegR], Speed * Time.deltaTime);

        if (!ValidatePoints(27)) return;
        Vector3 up27 = GetDirection(points[25], points[27]);
        Bones[BoneType.lowerLegR].LerpRotateTo(Quaternion.LookRotation(up27, up25) * InverseMap[BoneType.lowerLegR] * InitRotation[BoneType.lowerLegR], Speed * Time.deltaTime);

        if (!ValidatePoints(29, 31)) return;
        Vector3 footDir = points[27].v3 - GetAverage(points[29], points[31]);
        Vector3 foorUp = GetDirection(points[27], points[31]);
        Bones[BoneType.footR].LerpRotateTo(Quaternion.LookRotation(footDir, foorUp) * InverseMap[BoneType.footR] * InitRotation[BoneType.footR], Speed * Time.deltaTime);
    }
}

public struct BlazePose
{
    public float x, y, z, visibility;

    Vector3 _v3;

    bool isInit;

    void Init()
    {
        _v3 = new Vector3(-x, -y, z);

        isInit = true;
    }

    public Vector3 v3 { get { if (!isInit) Init(); return _v3; } }

    public bool isVisible(float threshold) { return visibility > threshold; }

    public BlazePose(float x, float y, float z, float visibility)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.visibility = visibility;
        _v3 = Vector3.zero;
        isInit = false;
    }

    public static BlazePose operator +(BlazePose p1, BlazePose p2) { return new BlazePose(p1.x + p2.x, p1.y + p2.y, p1.z + p2.z, p1.visibility + p2.visibility); }
    public static BlazePose operator /(BlazePose p, float factor) { return new BlazePose(p.x / factor, p.y / factor, p.z / factor, p.visibility / factor); }

}