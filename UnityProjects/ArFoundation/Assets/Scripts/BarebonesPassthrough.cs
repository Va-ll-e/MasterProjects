using UnityEngine;
using VIVE.OpenXR;
using VIVE.OpenXR.CompositionLayer;
using VIVE.OpenXR.Passthrough;
using XrPassthroughHTC = VIVE.OpenXR.Passthrough.XrPassthroughHTC;

public class BarebonesPassthrough : MonoBehaviour
{

    private XrPassthroughHTC passthroughHandle;
    private bool isCreated = false;

    void Start()
    {
        // Use the new API correctly
        XrResult result = PassthroughAPI.CreatePlanarPassthrough(out passthroughHandle, LayerType.Underlay);
        
        if (result == XrResult.XR_SUCCESS)
        {
            isCreated = true;
            Debug.Log("Passthrough created successfully.");
        }
        else
        {
            Debug.LogError($"Failed to create passthrough: {result}");
        }
    }

    void OnDestroy()
    {
        if (isCreated)
        {
            PassthroughAPI.DestroyPassthrough(passthroughHandle);
        }
    }
}

