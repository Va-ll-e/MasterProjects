using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class ArucoDetector : MonoBehaviour
{
    // DLL Import - must be static extern inside class
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
    public GameObject digitalTwin;           // Drag your Franka robot prefab here
    public float markerSizeMeters = 0.05f;   // Real size of your printed marker (e.g. 5 cm = 0.05)
    public int targetMarkerId = 42;          // The ID of the marker you want to track
    public GameObject targetMarker;
    
    [Header("Calibration - replace with real values later")]
    private readonly float[] cameraMatrix = new float[9]
    {
        1040.34510f,    0f,  838.193753f,     // fx, 0, cx   ← approximate for 1920x1080 webcam
        0f,    1039.9049300f,  526.1639999f,     // 0, fy, cy
        0f,       0f,    1f
    };

    private readonly float[] distCoeffs = new float[5]
    {
        0.184105721f,   // k1
        -0.312982724f,   // k2
        -0.000395015223f,// p1
        0.000304874727f,// p2
        0.194393586f     // k3
    };
    
    
    // Buffers
    private const int MAX_MARKERS = 5;
    private int[] ids = new int[MAX_MARKERS];
    private float[] tvecs = new float[MAX_MARKERS * 3];
    private float[] rvecs = new float[MAX_MARKERS * 3];

    // Webcam
    private WebCamTexture webcamTexture;
    private Texture2D readableTexture;  // temp copy for GetRawTextureData

    IEnumerator Start()
    {
        webcamTexture = new WebCamTexture(WebCamTexture.devices[0].name, 1920, 1080, 30);
        webcamTexture.Play();

        // Wait until webcam is initialized
        while (webcamTexture.width <= 16)
            yield return null;

        readableTexture = new Texture2D(
            webcamTexture.width,
            webcamTexture.height,
            TextureFormat.RGBA32,
            false);

        targetMarker.GetComponent<Renderer>().material.mainTexture = webcamTexture;
    }


    void Update()
    {
        if (!webcamTexture.isPlaying || !webcamTexture.didUpdateThisFrame)
            return;

        // Copy webcam pixels to readable texture (required for GetRawTextureData)
        readableTexture.SetPixels32(webcamTexture.GetPixels32());
        readableTexture.Apply();

        // Get raw RGBA32 bytes
        byte[] rawData = readableTexture.GetRawTextureData<byte>().ToArray();

        int detectedCount;
       // Debug.Log($"Frame size: {webcamTexture.width}x{webcamTexture.height}");
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
       // Debug.Log($"DLL returned: {result}, detected: {detectedCount}");

        if (result > 0 && detectedCount > 0)
        {
            // Check for your target marker (loop if you want all)
            for (int i = 0; i < detectedCount; i++)
            {
                if (ids[i] == targetMarkerId)
                {
                    // Position: OpenCV tvec is right-handed → often flip Z for Unity
                    Vector3 position = new Vector3(
                        tvecs[i * 3 + 0],
                        tvecs[i * 3 + 1],
                        -tvecs[i * 3 + 2]);  // ← common flip

                    const float MAX_REASONABLE_DISTANCE = 5f;   // meters
                    const float MIN_REASONABLE_DISTANCE = 0.1f;

                    // Rotation: rvec (Rodrigues) → Quaternion
                    // Quick approx (improve later with full Rodrigues conversion)
                    Vector3 rvec = new Vector3(
                        rvecs[i * 3 + 0],
                        rvecs[i * 3 + 1],
                        rvecs[i * 3 + 2]);

                    Quaternion rotation = RodriguesToQuaternion(rvec);

                    if (!float.IsNaN(rotation.x) &&
                        !float.IsNaN(position.x) && !float.IsNaN(position.y) && !float.IsNaN(position.z) &&
                        Mathf.Abs(position.x) < MAX_REASONABLE_DISTANCE &&
                        Mathf.Abs(position.y) < MAX_REASONABLE_DISTANCE &&
                        position.z is >= MIN_REASONABLE_DISTANCE and <= MAX_REASONABLE_DISTANCE)
                    {
                        digitalTwin.transform.localPosition = position;
                        digitalTwin.transform.localRotation = rotation;
                        Debug.Log($"VALID pose applied → Pos: {position}, Rot: {rotation.eulerAngles}");
                    }
                    else
                    {
                       // Debug.LogWarning($"Outlier pose rejected → Pos: {position} (likely numerical instability)");
                    }

                    //  Debug.Log($"Marker {targetMarkerId} detected! Pos: {position}");
                    break;
                }
            }
        }
    }

    // Simple Rodrigues → Quaternion conversion (add axis-angle logic)
    private Quaternion RodriguesToQuaternion(Vector3 rvec)
    {
        float angle = rvec.magnitude;

        // Handle near-zero rotation safely (common when marker is frontal)
        if (angle < 1e-5f)  // very small threshold
        {
            return Quaternion.identity;  // no rotation
        }

        // Normalize axis safely
        Vector3 axis = rvec / angle;  // divide by magnitude instead of .normalized

        // Convert to degrees and create quaternion
        Quaternion rot = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, axis);

        // Optional: small safety clamp to avoid any floating-point weirdness
        if (float.IsNaN(rot.x) || float.IsNaN(rot.y) || float.IsNaN(rot.z) || float.IsNaN(rot.w))
        {
          //  Debug.LogWarning("NaN rotation detected - falling back to identity");
            return Quaternion.identity;
        }

        return rot;
    }

    void OnDestroy()
    {
        if (webcamTexture != null) webcamTexture.Stop();
    }
}