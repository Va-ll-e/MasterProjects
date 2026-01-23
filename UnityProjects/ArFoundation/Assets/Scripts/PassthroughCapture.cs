using UnityEngine;
using VIVE.OpenXR.CompositionLayer;
using VIVE.OpenXR.Passthrough;

    public class PassthroughCapture : MonoBehaviour
    {
        // vive through settings
        private VIVE.OpenXR.Passthrough.XrPassthroughHTC activePassthroughID;
        private LayerType currentActiveLayerType = LayerType.Underlay;

        void Start()
        {
            PassthroughAPI.SetPassthroughLayerType(activePassthroughID, LayerType.Underlay);
            currentActiveLayerType = LayerType.Underlay;
            PassthroughAPI.CreatePlanarPassthrough(out activePassthroughID, currentActiveLayerType, OnDestroyPassthroughFeatureSession);
        }

        // Update is called once per frame
        void Update()
        {
            if(Time.realtimeSinceStartup > 10)
            {
                PassthroughAPI.DestroyPassthrough(activePassthroughID);
                activePassthroughID = 0;
            }
        }

        void OnDestroyPassthroughFeatureSession(VIVE.OpenXR.Passthrough.XrPassthroughHTC passthroughID)
        {
            PassthroughAPI.DestroyPassthrough(passthroughID);
            activePassthroughID = 0;
        }
    }
