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
    public GameObject digitalTwin;           // Drag your Franka prefab / Cube here (make it a child of this camera!)
    public float markerSizeMeters = 0.05f;   // Measure your real marker size!
    public int targetMarkerId = 42;

    [Header("Calibration - HoloLens values")]
    private readonly float[] cameraMatrix = new float[9]
    {
        3405.77286f,    0f,  1940.72687f,
        0f,          3400.28003f,  1103.21077f,
        0f,             0f,          1f
    };

    private readonly float[] distCoeffs = new float[5]
    {
        -0.220268529f, 5.85710669f, 0.00135260716f,
        0.00316794067f, -26.9110395f
    };

    // Buffers
    private const int MAX_MARKERS = 5;
    private int[] ids = new int[MAX_MARKERS];
    private float[] tvecs = new float[MAX_MARKERS * 3];
    private float[] rvecs = new float[MAX_MARKERS * 3];

    // Passthrough capture
    private RenderTexture passthroughRT;
    private Texture2D readableTexture;

    // Color feedback
    private Renderer twinRenderer;
    private Color defaultColor = Color.green;
    private Color detectedColor = Color.red;
    private float lastDetectionTime = -10f;  // time of last valid detection
    private const float RESET_DELAY = 1f;    // seconds without detection → revert to green

    void Start()
    {
        if (!GetComponent<Camera>())
        {
            Debug.LogError("ArucoDetector must be on the XR Main Camera!");
            enabled = false;
            return;
        }

        // Cache renderer once
        if (digitalTwin != null)
        {
            twinRenderer = digitalTwin.GetComponentInChildren<Renderer>(true);
            if (twinRenderer == null)
                Debug.LogError("Digital twin has no Renderer!");
            else
                twinRenderer.material.color = defaultColor;
        }

        // Capture setup - lower res for better performance on HoloLens
        passthroughRT = new RenderTexture(1280, 720, 0, RenderTextureFormat.ARGB32);
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

        // Capture passthrough
        Graphics.Blit(source, passthroughRT);

        // CPU readback
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

        bool tagDetectedThisFrame = false;

        if (result > 0 && detectedCount > 0)
        {
            for (int i = 0; i < detectedCount; i++)
            {
                if (ids[i] == targetMarkerId)
                {
                    tagDetectedThisFrame = true;
                    lastDetectionTime = Time.time;

                    // Optional: still apply pose if you want (comment out if only color matters)
                    Vector3 position = new Vector3(tvecs[i*3+0], tvecs[i*3+1], -tvecs[i*3+2]);
                    Quaternion rotation = RodriguesToQuaternion(new Vector3(rvecs[i*3+0], rvecs[i*3+1], rvecs[i*3+2]));

                    if (twinRenderer != null)
                        twinRenderer.material.color = detectedColor;

                    // Uncomment if you want position update too:
                    // digitalTwin.transform.localPosition = position;
                    // digitalTwin.transform.localRotation = rotation;

                    break;
                }
            }
        }

        // Reset color after delay if no tag seen recently
        if (!tagDetectedThisFrame && Time.time - lastDetectionTime > RESET_DELAY)
        {
            if (twinRenderer != null)
                twinRenderer.material.color = defaultColor;
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