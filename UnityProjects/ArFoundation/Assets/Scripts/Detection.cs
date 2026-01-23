using System.Collections;
using UnityEngine;

public class Detection : MonoBehaviour
{
        [Header("Setup")]
        public GameObject plane;
    
        private WebCamTexture webcamTexture;
        private Texture2D readableTexture;
        private Renderer boxRenderer;
        private Renderer planeRenderer;
    
        // Color feedback
        private Color detectedColor = Color.green;
    
        void Start()
        {
            planeRenderer = plane.GetComponent<Renderer>();
            
            webcamTexture = new WebCamTexture(WebCamTexture.devices[1].name, 1920, 1080, 30);
            webcamTexture.Play();
    
            StartCoroutine(WaitForWebcamReady());
            
            // Get the list of available camera devices
            var devices = WebCamTexture.devices;

            // Check if any devices were found
            if (devices.Length == 0)
            {
                Debug.Log("No webcams found!");
                return;
            }

            Debug.Log($"Found {devices.Length} webcams:");

            // Iterate through each device and log its information
            for (int i = 0; i < devices.Length; i++)
            {
                WebCamDevice device = devices[i];
                Debug.Log($"Device {i}: Name='{device.name}', Front Facing={device.isFrontFacing}");
                // You can also check for other properties like device.kind if needed
            }
        }
    
        IEnumerator WaitForWebcamReady()
        {
            while (webcamTexture.width <= 16)
                yield return null;
    
            planeRenderer.material.mainTexture = webcamTexture;
        }
    
        void Update()
        {
            if (!webcamTexture.isPlaying || !webcamTexture.didUpdateThisFrame)
                return;
        }
    
        void OnDestroy()
        {
            if (webcamTexture != null && webcamTexture.isPlaying)
                webcamTexture.Stop();
            if (readableTexture != null)
                Destroy(readableTexture);
        }
    }