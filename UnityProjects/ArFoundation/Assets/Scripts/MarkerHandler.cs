using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MarkerHandler : MonoBehaviour
{
    [SerializeField] private ARMarkerManager markerManager;
    [SerializeField] private GameObject prefabToPlace;  // Assign your red cube here

    private Dictionary<TrackableId, GameObject> spawnedObjects = new();

    void OnEnable()
    {
        if (markerManager == null)
            markerManager = GetComponent<ARMarkerManager>();

        markerManager.trackablesChanged.AddListener(OnTrackablesChanged);
    }

    void OnDisable()
    {
        markerManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
    }

    void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARMarker> eventArgs)
    {
        // Added markers
        foreach (var marker in eventArgs.added)
        {
            Debug.Log($"ArUco detected! ID: {marker.markerId}, Tracking State: {marker.trackingState}");

            if (prefabToPlace != null)
            {
                GameObject go = Instantiate(prefabToPlace, marker.transform.position, marker.transform.rotation);
                go.transform.SetParent(marker.transform);  // Parent for automatic following
                spawnedObjects[marker.trackableId] = go;
            }
        }

        // Updated markers
        foreach (var marker in eventArgs.updated)
        {
            if (spawnedObjects.TryGetValue(marker.trackableId, out var go))
            {
                go.transform.position = marker.transform.position;
                go.transform.rotation = marker.transform.rotation;
            }
        }

        // Removed markers
        foreach (var removedPair in eventArgs.removed)
        {
            TrackableId trackableId = removedPair.Key;
            // ARMarker marker = removedPair.Value;  // Optional: use if needed (e.g., for final data)

            if (spawnedObjects.TryGetValue(trackableId, out var go))
            {
                Destroy(go);
                spawnedObjects.Remove(trackableId);
            }
        }
    }
}