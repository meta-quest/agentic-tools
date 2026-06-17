// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// OVRCustomFace subclass that maps ARKit-named blendshapes (camelCase + _L/_R, with an
/// optional prefix like "blendShape2.") to OVR FaceExpressions. Mapping is built directly
/// from the mesh blendshape names in MapBlendshapes() — call via context menu in the
/// inspector, or it runs automatically on Reset and from OnValidate when _mappings is empty.
/// </summary>
public class ARKitOVRCustomFace : OVRCustomFace
{
    static readonly (string, OVRFaceExpressions.FaceExpression)[] ARKitTable = new[]
    {
        ("browDown_L",       OVRFaceExpressions.FaceExpression.BrowLowererL),
        ("browDown_R",       OVRFaceExpressions.FaceExpression.BrowLowererR),
        ("browInnerUp",      OVRFaceExpressions.FaceExpression.InnerBrowRaiserL),
        ("browOuterUp_L",    OVRFaceExpressions.FaceExpression.OuterBrowRaiserL),
        ("browOuterUp_R",    OVRFaceExpressions.FaceExpression.OuterBrowRaiserR),
        ("cheekPuff",        OVRFaceExpressions.FaceExpression.CheekPuffL),
        ("cheekSquint_L",    OVRFaceExpressions.FaceExpression.CheekRaiserL),
        ("cheekSquint_R",    OVRFaceExpressions.FaceExpression.CheekRaiserR),
        ("eyeBlink_L",       OVRFaceExpressions.FaceExpression.EyesClosedL),
        ("eyeBlink_R",       OVRFaceExpressions.FaceExpression.EyesClosedR),
        ("eyeLookDown_L",    OVRFaceExpressions.FaceExpression.EyesLookDownL),
        ("eyeLookDown_R",    OVRFaceExpressions.FaceExpression.EyesLookDownR),
        ("eyeLookIn_L",      OVRFaceExpressions.FaceExpression.EyesLookRightL),
        ("eyeLookIn_R",      OVRFaceExpressions.FaceExpression.EyesLookLeftR),
        ("eyeLookOut_L",     OVRFaceExpressions.FaceExpression.EyesLookLeftL),
        ("eyeLookOut_R",     OVRFaceExpressions.FaceExpression.EyesLookRightR),
        ("eyeLookUp_L",      OVRFaceExpressions.FaceExpression.EyesLookUpL),
        ("eyeLookUp_R",      OVRFaceExpressions.FaceExpression.EyesLookUpR),
        ("eyeSquint_L",      OVRFaceExpressions.FaceExpression.LidTightenerL),
        ("eyeSquint_R",      OVRFaceExpressions.FaceExpression.LidTightenerR),
        ("eyeWide_L",        OVRFaceExpressions.FaceExpression.UpperLidRaiserL),
        ("eyeWide_R",        OVRFaceExpressions.FaceExpression.UpperLidRaiserR),
        ("jawForward",       OVRFaceExpressions.FaceExpression.JawThrust),
        ("jawLeft",          OVRFaceExpressions.FaceExpression.JawSidewaysLeft),
        ("jawOpen",          OVRFaceExpressions.FaceExpression.JawDrop),
        ("jawRight",         OVRFaceExpressions.FaceExpression.JawSidewaysRight),
        ("mouthClose",       OVRFaceExpressions.FaceExpression.LipsToward),
        ("mouthDimple_L",    OVRFaceExpressions.FaceExpression.DimplerL),
        ("mouthDimple_R",    OVRFaceExpressions.FaceExpression.DimplerR),
        ("mouthFrown_L",     OVRFaceExpressions.FaceExpression.LipCornerDepressorL),
        ("mouthFrown_R",     OVRFaceExpressions.FaceExpression.LipCornerDepressorR),
        ("mouthFunnel",      OVRFaceExpressions.FaceExpression.LipFunnelerLT),
        ("mouthLeft",        OVRFaceExpressions.FaceExpression.MouthLeft),
        ("mouthLowerDown_L", OVRFaceExpressions.FaceExpression.LowerLipDepressorL),
        ("mouthLowerDown_R", OVRFaceExpressions.FaceExpression.LowerLipDepressorR),
        ("mouthPress_L",     OVRFaceExpressions.FaceExpression.LipPressorL),
        ("mouthPress_R",     OVRFaceExpressions.FaceExpression.LipPressorR),
        ("mouthPucker",      OVRFaceExpressions.FaceExpression.LipPuckerL),
        ("mouthRight",       OVRFaceExpressions.FaceExpression.MouthRight),
        ("mouthRollLower",   OVRFaceExpressions.FaceExpression.LipSuckLB),
        ("mouthRollUpper",   OVRFaceExpressions.FaceExpression.LipSuckLT),
        ("mouthShrugLower",  OVRFaceExpressions.FaceExpression.ChinRaiserB),
        ("mouthShrugUpper",  OVRFaceExpressions.FaceExpression.ChinRaiserT),
        ("mouthSmile_L",     OVRFaceExpressions.FaceExpression.LipCornerPullerL),
        ("mouthSmile_R",     OVRFaceExpressions.FaceExpression.LipCornerPullerR),
        ("mouthStretch_L",   OVRFaceExpressions.FaceExpression.LipStretcherL),
        ("mouthStretch_R",   OVRFaceExpressions.FaceExpression.LipStretcherR),
        ("mouthUpperUp_L",   OVRFaceExpressions.FaceExpression.UpperLipRaiserL),
        ("mouthUpperUp_R",   OVRFaceExpressions.FaceExpression.UpperLipRaiserR),
        ("noseSneer_L",      OVRFaceExpressions.FaceExpression.NoseWrinklerL),
        ("noseSneer_R",      OVRFaceExpressions.FaceExpression.NoseWrinklerR),
        ("tongueOut",        OVRFaceExpressions.FaceExpression.TongueOut),
    };

    protected override (string[], OVRFaceExpressions.FaceExpression[])
        GetCustomBlendShapeNameAndExpressionPairs()
    {
        var names = new string[ARKitTable.Length];
        var exprs = new OVRFaceExpressions.FaceExpression[ARKitTable.Length];
        for (int i = 0; i < ARKitTable.Length; i++)
        {
            names[i] = ARKitTable[i].Item1;
            exprs[i] = ARKitTable[i].Item2;
        }
        return (names, exprs);
    }

    [ContextMenu("Map Blendshapes")]
    public void MapBlendshapes()
    {
        var smr = GetComponent<SkinnedMeshRenderer>();
        if (smr == null || smr.sharedMesh == null)
        {
            Debug.LogError($"[ARKitOVRCustomFace] no SkinnedMeshRenderer/mesh on {name}", this);
            return;
        }
        var mesh = smr.sharedMesh;

        var lookup = new Dictionary<string, OVRFaceExpressions.FaceExpression>(ARKitTable.Length);
        foreach (var (n, e) in ARKitTable) lookup[n.ToLowerInvariant()] = e;

        int n2 = mesh.blendShapeCount;
        var mappings = new OVRFaceExpressions.FaceExpression[n2];
        int matched = 0;
        for (int i = 0; i < n2; i++)
        {
            var raw = mesh.GetBlendShapeName(i);
            var dot = raw.LastIndexOf('.');
            var key = (dot >= 0 ? raw.Substring(dot + 1) : raw).ToLowerInvariant();
            if (lookup.TryGetValue(key, out var fe))
            {
                mappings[i] = fe;
                matched++;
            }
            else
            {
                mappings[i] = OVRFaceExpressions.FaceExpression.Max;
            }
        }
        Mappings = mappings;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        Debug.Log($"[ARKitOVRCustomFace] mapped {matched}/{n2} blendshapes on '{mesh.name}'", this);
    }

    void Reset()
    {
        MapBlendshapes();
    }

    void OnValidate()
    {
        if (Mappings == null || Mappings.Length == 0)
            MapBlendshapes();
    }
}
