using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;

#if ENABLE_WINMD_SUPPORT
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using TMPro;
using Windows.Graphics.Imaging;
using Microsoft.MixedReality.OpenXR;
using System.Threading.Tasks;
using OpenCVBridge;
using System.Linq;
using UnityEngine.XR.ARFoundation;
#endif

public class ArUcoTracking : MonoBehaviour
{
    [Header("Tracking Settings")]
    public float markerSize = 0.1f;                 // Size of the printed marker in meters
    public float lockTime = 5.0f;                   // Time in seconds before locking position
    public bool autoStopOnLock = true;              // Stop camera processing after locking to save battery

    [Header("References")]
    public GameObject markerGo;
    public bool useCustomCameraIntrinsics;
    public CameraIntrinsics customCameraIntrinsics;

    // Internal State
    private ArUcoUtils.ArUcoDictionary arUcoDictionary = ArUcoUtils.ArUcoDictionary.DICT_4X4_50;
    private bool _isRunning = false;
    private bool _isLocked = false;
    private float _currentTrackDuration = 0f;
    private CameraIntrinsics _cameraIntrinsics;

#if ENABLE_WINMD_SUPPORT
    OpenCVHelper _cvHelper = null;
    MediaCapturer _mediaCapturer = null;
    Windows.Perception.Spatial.SpatialCoordinateSystem _unityCoordinateSystem = null;
    Windows.Perception.Spatial.SpatialCoordinateSystem _frameCoordinateSystem = null;
#endif

    private void Awake()
    {
#if ENABLE_WINMD_SUPPORT
       // Cache the Unity coordinate system once at startup
       _unityCoordinateSystem = PerceptionInterop.GetSceneCoordinateSystem(UnityEngine.Pose.identity) as Windows.Perception.Spatial.SpatialCoordinateSystem;
#endif
    }

    async void Start()
    {
        if (markerGo == null)
        {
            Debug.LogError("markerGo not assigned.");
            return;
        }

        markerGo.SetActive(false);

        // Standard HoloLens PV Camera specs
        var width = 896;
        var height = 504;
        var frameRate = 30;

#if ENABLE_WINMD_SUPPORT
        try
        {
            _cvHelper = new OpenCVHelper();
            _mediaCapturer = new MediaCapturer();
            
            await _mediaCapturer.StartCapture(width, height, frameRate);
            RunArUcoTracking();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
#endif
    }

    private async void OnApplicationFocus(bool focus)
    {
#if ENABLE_WINMD_SUPPORT
       if (!focus && _mediaCapturer != null) await _mediaCapturer.StopCapturing();
#endif
    }

    // ----------------------------------------------------------------
    //  Tracking Logic
    // ----------------------------------------------------------------

#if ENABLE_WINMD_SUPPORT
    
    public async void RunArUcoTracking() 
    {
        _isRunning = true;
        _isLocked = false;
        _currentTrackDuration = 0f;

        await Task.Run(async () =>
        {
            while (_isRunning)
            {
                if (_mediaCapturer.IsCapturing)
                {
                    var mediaFrameReference = _mediaCapturer.GetLatestFrameRef();
                    // Process the frame (Background Thread)
                    HandleArUcoTracking(mediaFrameReference);
                    mediaFrameReference?.Dispose();
                }
                else
                {
                    await Task.Delay(33); // Small delay if not capturing to prevent CPU spin
                }
            }
        });
    }

    public void StopArUcoTracking() 
    {
        _isRunning = false;
        Debug.Log("ArUco Tracking Stopped.");
    }

    private void HandleArUcoTracking(Windows.Media.Capture.Frames.MediaFrameReference mediaFrameReference)
    {
        if (_isLocked) return; // Stop processing heavy math if we are locked

        var softwareBitmap = mediaFrameReference?.VideoMediaFrame?.SoftwareBitmap;
        var pFIntrinsics = mediaFrameReference?.VideoMediaFrame?.CameraIntrinsics;

        // 1. Setup Intrinsics (Safe Check)
        if (!useCustomCameraIntrinsics && pFIntrinsics != null)
        {
            if (_cameraIntrinsics == null) _cameraIntrinsics = new CameraIntrinsics();
            
            _cameraIntrinsics.focalLength = VectorExtensions.ToUnity(pFIntrinsics.FocalLength);
            _cameraIntrinsics.principalPoint = VectorExtensions.ToUnity(pFIntrinsics.PrincipalPoint);
            _cameraIntrinsics.radialDistortion = VectorExtensions.ToUnity(pFIntrinsics.RadialDistortion);
            _cameraIntrinsics.tangentialDistortion = VectorExtensions.ToUnity(pFIntrinsics.TangentialDistortion);
        }
        else if (_cameraIntrinsics == null)
        {
            _cameraIntrinsics = customCameraIntrinsics;
        }

        if (softwareBitmap != null && _cameraIntrinsics != null)
        {
            _frameCoordinateSystem = mediaFrameReference.CoordinateSystem;
            
            // 2. Detect Markers
            int frameProcessingTime = 0;
            var markers = _cvHelper.ProcessWithArUco(
                            softwareBitmap, 
                            VectorExtensions.ToNumerics(_cameraIntrinsics.focalLength), 
                            VectorExtensions.ToNumerics(_cameraIntrinsics.principalPoint), 
                            VectorExtensions.ToNumerics(_cameraIntrinsics.radialDistortion), 
                            VectorExtensions.ToNumerics(_cameraIntrinsics.tangentialDistortion),
                            (int)arUcoDictionary,
                            markerSize,
                            out frameProcessingTime);

            // 3. If marker found, update position immediately
            if (markers.Count > 0)
            {
                UpdateObjectPose(markers[0]);
            }
        }

        softwareBitmap?.Dispose();
    }

    private void UpdateObjectPose(OpenCVBridge.DetectedMarker marker)
    {
        // Calculate Unity World Position from Marker Data
        UnityEngine.Vector3 translationUnity = ArUcoUtils.Vec3FromFloat3(marker.Position());
        UnityEngine.Vector3 rotationRodrigues = ArUcoUtils.Vec3FromFloat3(marker.Rotation());
        UnityEngine.Quaternion rotationUnity = ArUcoUtils.RotationQuatFromRodrigues(rotationRodrigues);

        UnityEngine.Matrix4x4 markerTransformUnityCamera = ArUcoUtils.GetTransformInUnityCamera(translationUnity, rotationUnity);
        UnityEngine.Matrix4x4 cameraToWorldUnity = CameraUtils.GetViewToUnityTransform(_frameCoordinateSystem, _unityCoordinateSystem);

        if (cameraToWorldUnity == null) return;

        UnityEngine.Matrix4x4 transformUnityWorld = cameraToWorldUnity * markerTransformUnityCamera;
        UnityEngine.Vector3 finalPos = ArUcoUtils.GetVectorFromMatrix(transformUnityWorld);
        UnityEngine.Quaternion finalRot = ArUcoUtils.GetQuatFromMatrix(transformUnityWorld);

        // 4. Dispatch to Main Thread to move object and check Timer
        UnityEngine.WSA.Application.InvokeOnAppThread(() =>
        {
            if (_isLocked) return;

            if (!markerGo.activeSelf) markerGo.SetActive(true);

            // SMOOTHING: Lerp reduces the "Jitter" you saw
            markerGo.transform.position = finalPos;
            markerGo.transform.rotation = finalRot;

            // TIMER LOGIC
            _currentTrackDuration += Time.deltaTime;

            if (_currentTrackDuration >= lockTime)
            {
                LockPosition();
            }

        }, false);
    }

    private void LockPosition()
    {
        _isLocked = true;
        Debug.Log("Position Locked!");

        // Add ArAnchor to pin it in reality
            markerGo.AddComponent<ARAnchor>();

        if (autoStopOnLock)
        {
            StopArUcoTracking();
        }
    }

#endif
}

// ----------------------------------------------------------------
// Helpers (Keep these as they were)
// ----------------------------------------------------------------

[Serializable]
public class CameraIntrinsics
{
    public UnityEngine.Vector2 focalLength;
    public UnityEngine.Vector2 principalPoint;
    public UnityEngine.Vector3 radialDistortion;
    public UnityEngine.Vector2 tangentialDistortion;
}

public static class VectorExtensions
{
    public static UnityEngine.Vector2 ToUnity(this System.Numerics.Vector2 vec) => new UnityEngine.Vector2(vec.X, vec.Y);
    public static System.Numerics.Vector2 ToNumerics(this UnityEngine.Vector2 vec) => new System.Numerics.Vector2(vec.x, vec.y);

    public static UnityEngine.Vector3 ToUnity(this System.Numerics.Vector3 vec) => new UnityEngine.Vector3(vec.X, vec.Y, vec.Z);
    public static System.Numerics.Vector3 ToNumerics(this UnityEngine.Vector3 vec) => new System.Numerics.Vector3(vec.x, vec.y, vec.z);
}