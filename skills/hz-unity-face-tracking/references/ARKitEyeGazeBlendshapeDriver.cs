// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using UnityEngine;

/// <summary>
/// Reads gaze rotation from two OVREyeGaze components (one per eye) and writes it into
/// a SkinnedMeshRenderer's ARKit eyeLookUp/Down/In/Out_L/R blendshapes.
///
/// Why this exists: the eye-related fields on OVRFaceExpressions (EyesLookUpL etc.) are
/// derived from face-camera visuals, not the dedicated eye tracker. On Quest Pro they're
/// often zero or unreliable. OVREyeGaze taps the eye tracker directly and gives a clean
/// per-eye orientation. This component runs in LateUpdate so it overrides the eye-shape
/// weights that OVRCustomFace/ARKitOVRCustomFace just wrote in Update.
///
/// Setup:
///   1. Create two empty GameObjects, parent them under the head or camera rig.
///      Add OVREyeGaze to each. Set Eye=Left/Right, TrackingMode=HeadSpace.
///      Optionally set ReferenceFrame = your head transform (CenterEyeAnchor works).
///   2. Add this component to the same GameObject as the head SkinnedMeshRenderer.
///   3. Wire leftEye, rightEye, and (optionally) referenceFrame in the inspector.
///   4. Add QuestFacePermissionsRequester to the startup scene — eye tracking won't
///      stream data without the runtime EYE_TRACKING permission request.
/// </summary>
[RequireComponent(typeof(SkinnedMeshRenderer))]
public class ARKitEyeGazeBlendshapeDriver : MonoBehaviour
{
    [SerializeField] OVREyeGaze leftEye;
    [SerializeField] OVREyeGaze rightEye;

    [SerializeField, Tooltip("Reference (head) transform. Gaze rotation is taken relative to this. " +
        "Leave empty to use Camera.main.")]
    Transform referenceFrame;

    [SerializeField, Tooltip("Eye rotation (degrees) at which the corresponding blendshape reaches weight 1.")]
    float maxAngleDeg = 30f;

    [SerializeField, Tooltip("Prefix on the mesh's blendshape names, e.g. 'blendShape2.'. Leave empty for none.")]
    string blendShapePrefix = "blendShape2.";

    [SerializeField, Tooltip("Smoothing factor (0 = no smoothing, 1 = freeze).")]
    [Range(0f, 0.95f)] float smoothing = 0.4f;

    SkinnedMeshRenderer _smr;
    int _upL, _downL, _inL, _outL, _upR, _downR, _inR, _outR;
    float _upLW, _downLW, _inLW, _outLW, _upRW, _downRW, _inRW, _outRW;

    void Awake()
    {
        _smr = GetComponent<SkinnedMeshRenderer>();
        var m = _smr.sharedMesh;
        _upL = m.GetBlendShapeIndex(blendShapePrefix + "eyeLookUp_L");
        _downL = m.GetBlendShapeIndex(blendShapePrefix + "eyeLookDown_L");
        _inL = m.GetBlendShapeIndex(blendShapePrefix + "eyeLookIn_L");
        _outL = m.GetBlendShapeIndex(blendShapePrefix + "eyeLookOut_L");
        _upR = m.GetBlendShapeIndex(blendShapePrefix + "eyeLookUp_R");
        _downR = m.GetBlendShapeIndex(blendShapePrefix + "eyeLookDown_R");
        _inR = m.GetBlendShapeIndex(blendShapePrefix + "eyeLookIn_R");
        _outR = m.GetBlendShapeIndex(blendShapePrefix + "eyeLookOut_R");
    }

    void LateUpdate()
    {
        var refT = referenceFrame != null ? referenceFrame
                                          : (Camera.main != null ? Camera.main.transform : null);
        if (refT == null) return;

        ApplyEye(leftEye, refT, isLeftEye: true,
            _upL, _downL, _inL, _outL,
            ref _upLW, ref _downLW, ref _inLW, ref _outLW);
        ApplyEye(rightEye, refT, isLeftEye: false,
            _upR, _downR, _inR, _outR,
            ref _upRW, ref _downRW, ref _inRW, ref _outRW);
    }

    void ApplyEye(OVREyeGaze gaze, Transform refT, bool isLeftEye,
                  int up, int down, int inIdx, int outIdx,
                  ref float upW, ref float downW, ref float inW, ref float outW)
    {
        if (gaze == null || !gaze.EyeTrackingEnabled) return;

        Quaternion rel = Quaternion.Inverse(refT.rotation) * gaze.transform.rotation;
        Vector3 e = rel.eulerAngles;
        float pitch = Wrap180(e.x); // +down, -up (Unity X-axis rotation: nose toward floor)
        float yaw = Wrap180(e.y); // +right, -left

        float targetUp = Mathf.Clamp01(-pitch / maxAngleDeg);
        float targetDown = Mathf.Clamp01(pitch / maxAngleDeg);
        // ARKit: "_In" = toward the nose. Left eye looks right to look in; right eye looks left.
        float targetIn = isLeftEye ? Mathf.Clamp01(yaw / maxAngleDeg)
                                    : Mathf.Clamp01(-yaw / maxAngleDeg);
        float targetOut = isLeftEye ? Mathf.Clamp01(-yaw / maxAngleDeg)
                                    : Mathf.Clamp01(yaw / maxAngleDeg);

        float k = smoothing;
        upW = Mathf.Lerp(targetUp, upW, k);
        downW = Mathf.Lerp(targetDown, downW, k);
        inW = Mathf.Lerp(targetIn, inW, k);
        outW = Mathf.Lerp(targetOut, outW, k);

        if (up >= 0) _smr.SetBlendShapeWeight(up, upW * 100f);
        if (down >= 0) _smr.SetBlendShapeWeight(down, downW * 100f);
        if (inIdx >= 0) _smr.SetBlendShapeWeight(inIdx, inW * 100f);
        if (outIdx >= 0) _smr.SetBlendShapeWeight(outIdx, outW * 100f);
    }

    static float Wrap180(float a) => a > 180f ? a - 360f : a;
}
