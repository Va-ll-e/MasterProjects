using UnityEngine;
using System.Runtime.InteropServices;

public class ArucoDetector : MonoBehaviour
{
    // DLL Import
    [DllImport("OpenCVArUcoWrapper", CallingConvention = CallingConvention.Cdecl)]
    private static extern int DetectArucoMarkers(
        byte[] imageData,
        int width,
        int height,
        float markerSizeMeters,
        float[] cameraMatrixFlat,
        float[] distCoeffsFlat,
        out int idsCount,
        int[] outIds,
        float[] outTvecs,
        float[] outRvecs);

    [Header("Setup")]
    public GameObject digitalTwin;           // Drag your Franka prefab here (make it a child of this camera!)
    public float markerSizeMeters = 0.05f;   // Measure your real marker size!
    public int targetMarkerId = 42;

    [Header("Calibration - your real values")]
    private readonly float[] cameraMatrix = new float[9]
    {
        1040.34510f,    0f,  838.192753f,
        0f,          1039.90493f,  526.163999f,
        0f,             0f,          1f
    };

    private readonly float[] distCoeffs = new float[5]
    {
        0.184105721f, -0.312982724f, -0.000395015223f,
        0.000304874727f, 0.194393586f
    };

    // Buffers
    private const int MAX_MARKERS = 5;
    private int[] ids = new int[MAX_MARKERS];
    private float[] tvecs = new float[MAX_MARKERS * 3];
    private float[] rvecs = new float[MAX_MARKERS * 3];

    // Passthrough capture
    private RenderTexture passthroughRT;
    private Texture2D readableTexture;

    // Smoothing
    private Vector3 smoothedPos = Vector3.zero;
    private Quaternion smoothedRot = Quaternion.identity;
    [Range(0.1f, 0.9f)] public float smoothFactor = 0.6f;  // higher = more responsive, lower = smoother

    void Start()
    {
        if (!GetComponent<Camera>())
        {
            Debug.LogError("ArucoDetector must be attached to a Camera (XR Main Camera)!");
            enabled = false;
            return;
        }

        // Create capture textures (match your headset resolution or use a reasonable size)
        passthroughRT = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
        passthroughRT.Create();

        readableTexture = new Texture2D(passthroughRT.width, passthroughRT.height, TextureFormat.RGBA32, false);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!enabled) 
        {
            Graphics.Blit(source, destination);
            return;
        }

        // Capture current passthrough frame
        Graphics.Blit(source, passthroughRT);

        // Read pixels to CPU-readable texture
        RenderTexture.active = passthroughRT;
        readableTexture.ReadPixels(new Rect(0, 0, passthroughRT.width, passthroughRT.height), 0, 0);
        readableTexture.Apply();

        byte[] rawData = readableTexture.GetRawTextureData<byte>().ToArray();

        int detectedCount;
        int result = DetectArucoMarkers(
            rawData,
            readableTexture.width,
            readableTexture.height,
            markerSizeMeters,
            cameraMatrix,
            distCoeffs,
            out detectedCount,
            ids,
            tvecs,
            rvecs);

        if (result > 0 && detectedCount > 0)
        {
            bool foundValid = false;

            for (int i = 0; i < detectedCount; i++)
            {
                if (ids[i] == targetMarkerId)
                {
                    Vector3 rawPos = new Vector3(tvecs[i*3+0], tvecs[i*3+1], -tvecs[i*3+2]);
                    Vector3 rvec = new Vector3(rvecs[i*3+0], rvecs[i*3+1], rvecs[i*3+2]);
                    Quaternion rawRot = RodriguesToQuaternion(rvec);

                    // Safety bounds (tune these based on your room scale)
                    if (!float.IsNaN(rawRot.x) &&
                        Mathf.Abs(rawPos.x) < 10f && Mathf.Abs(rawPos.y) < 10f &&
                        rawPos.z >= 0.05f && rawPos.z < 10f)
                    {
                        // Smooth
                        smoothedPos = Vector3.Lerp(smoothedPos, rawPos, smoothFactor);
                        smoothedRot = Quaternion.Slerp(smoothedRot, rawRot, smoothFactor);

                        digitalTwin.transform.localPosition = smoothedPos;
                        digitalTwin.transform.localRotation = smoothedRot;

                        Debug.Log($"VALID overlay → Pos: {smoothedPos}, Rot: {smoothedRot.eulerAngles}");
                        foundValid = true;
                        break;
                    }
                }
            }

            if (!foundValid)
            {
                Debug.Log("Detected marker but pose out of bounds - skipped");
            }
        }

        // Forward to display
        Graphics.Blit(source, destination);
    }

    private Quaternion RodriguesToQuaternion(Vector3 rvec)
    {
        float angle = rvec.magnitude;
        if (angle < 1e-5f) return Quaternion.identity;

        Vector3 axis = rvec / angle;
        Quaternion rot = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, axis);

        if (float.IsNaN(rot.x) || float.IsNaN(rot.y) || float.IsNaN(rot.z) || float.IsNaN(rot.w))
            return Quaternion.identity;

        return rot;
    }

    void OnDestroy()
    {
        if (passthroughRT != null) passthroughRT.Release();
        if (readableTexture != null) Destroy(readableTexture);
    }
}