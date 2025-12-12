using System;
using System.Threading;
using UnityEngine;
using ZXing;
using ZXing.Common;

public class QrReader : MonoBehaviour
{
    [SerializeField] private GameObject digitalTwinRoot;
    [SerializeField] private float qrPhysicalSize = 0.12f;
    [SerializeField] private float updateInterval = 1f; // 1 Hz

    private WebCamTexture camTexture;
    private Color32[] latestFrame;
    private int latestWidth, latestHeight;
    private volatile bool newFrameAvailable = false;
    private bool isRunning = true;

    private readonly IBarcodeReader barcodeReader = new BarcodeReader
    {
        AutoRotate = true,
        Options = new DecodingOptions { TryHarder = true }
    };

    void Awake()
    {
        // Create dispatcher on main thread — this is the ONLY safe place
        UnityMainThreadDispatcher.EnsureCreated();
    }

    void Start()
    {
        camTexture = new WebCamTexture(640, 480, 15);
        camTexture.Play();

        new Thread(DetectionThread) { IsBackground = true }.Start();
    }

    void Update()
    {
        if (camTexture.isPlaying && camTexture.width > 100)
        {
            latestFrame = camTexture.GetPixels32();
            latestWidth = camTexture.width;
            latestHeight = camTexture.height;
            newFrameAvailable = true;
        }
    }

    void DetectionThread()
    {
        while (isRunning)
        {
            Thread.Sleep((int)(updateInterval * 1000));

            if (!newFrameAvailable) continue;

            Color32[] frame = latestFrame;
            int w = latestWidth;
            int h = latestHeight;
            newFrameAvailable = false;

            byte[] rgb = new byte[w * h * 3];
            int idx = 0;
            for (int i = 0; i < frame.Length; i++)
            {
                rgb[idx++] = frame[i].r;
                rgb[idx++] = frame[i].g;
                rgb[idx++] = frame[i].b;
            }

            var luminance = new RGBLuminanceSource(rgb, w, h, RGBLuminanceSource.BitmapFormat.RGB24);
            var result = barcodeReader.Decode(luminance);

            if (result != null && result.BarcodeFormat == BarcodeFormat.QR_CODE && result.ResultPoints?.Length >= 3)
            {
                var corners = result.ResultPoints;
                int imgW = w;
                int imgH = h;
                UnityMainThreadDispatcher.Instance.Enqueue(() => AlignToQr(corners, imgW, imgH));
            }
        }
    }

    void AlignToQr(ResultPoint[] corners, int imgWidth, int imgHeight)
    {
        Vector2[] pts = new Vector2[corners.Length];
        for (int i = 0; i < corners.Length; i++)
            pts[i] = new Vector2(corners[i].X, imgHeight - corners[i].Y);

        if (pts.Length == 3) pts = new Vector2[] { pts[0], pts[1], pts[2], pts[0] };

        Vector2 center = Vector2.zero;
        for (int i = 0; i < pts.Length; i++) center += pts[i];
        center /= pts.Length;

        float pixelSize = Vector2.Distance(pts[0], pts[1]);
        float distance = (qrPhysicalSize * (imgWidth * 0.8f)) / pixelSize;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(center.x, center.y, distance));

        Vector3 edge = new Vector3(pts[1].x - pts[0].x, pts[1].y - pts[0].y, 0);
        Quaternion rot = Quaternion.LookRotation(Vector3.forward, edge);

        digitalTwinRoot.transform.position = worldPos;
        digitalTwinRoot.transform.rotation = rot;
        digitalTwinRoot.SetActive(true);

        Debug.Log("Digital twin aligned — no lag!");
    }

    void OnDestroy()
    {
        isRunning = false;
        camTexture?.Stop();
    }
}