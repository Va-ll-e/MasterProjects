using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections;

public class TagDetection : MonoBehaviour
{
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
    public GameObject box;
    public float markerSizeMeters = 0.05f;
    public GameObject plane;

    [Header("Calibration")]
    private readonly float[] cameraMatrix = new float[9]
    {
        3405.77286f, 0f, 1940.72687f,
        0f, 3400.28003f, 1103.21077f,
        0f, 0f, 1f
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

    private WebCamTexture webcamTexture;
    private Texture2D readableTexture;
    private Renderer boxRenderer;
    private Renderer planeRenderer;

    // Color feedback
    private Color detectedColor = Color.green;

    void Start()
    {
        boxRenderer = box.GetComponent<Renderer>();
        planeRenderer = plane.GetComponent<Renderer>();

        webcamTexture = new WebCamTexture(WebCamTexture.devices[0].name, 1920, 1080, 30);
        webcamTexture.Play();

        StartCoroutine(WaitForWebcamReady());
    }

    IEnumerator WaitForWebcamReady()
    {
        while (webcamTexture.width <= 16)
            yield return null;

        readableTexture = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.RGBA32, false);

        
        planeRenderer.material.mainTexture = webcamTexture;
    }

    void Update()
    {
        if (!webcamTexture.isPlaying || !webcamTexture.didUpdateThisFrame)
            return;

        // Copy current frame
        readableTexture.SetPixels32(webcamTexture.GetPixels32());
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
            rvecs
            );
        
        if (result > 0 && detectedCount > 0)
        {
            boxRenderer.material.color = detectedColor;
        }
        else
        {
            boxRenderer.material.color = Color.white;
        }
    }

    void OnDestroy()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
            webcamTexture.Stop();
        if (readableTexture != null)
            Destroy(readableTexture);
    }
}