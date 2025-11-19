using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI.BodyUI;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace Hand_UI
{
    /// <summary>
    /// Attaches a UI Toolkit menu to the left hand, positioning it when the palm is facing up and hiding it by moving it far away when not needed.
    /// </summary>
    /// <remarks>
    /// Assumes the left palm anchor (Tracked Pose Driver) has y-axis up, z-axis forward, x-axis right when palm is down (OpenXR).
    /// Uses FollowPresetDatumProperty to adjust tracking for hand or controller input.
    /// </remarks>
    [AddComponentMenu("XR/Left Hand Menu V2", 22)]
    public class LeftHandMenu : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The UI GameObject with UIDocument to show/hide and position on the left hand.")]
        private GameObject handMenuUIGameObject;

        [SerializeField]
        [Tooltip("Tracked Pose Driver for the left palm's position, rotation, and tracking state.")]
        private Transform leftPalmAnchor;

        [SerializeField]
        [Tooltip("Local offset for the menu relative to the palm anchor (in meters).")]
        private Vector3 menuOffset = new Vector3(0f, 0.03f, 0.02f);

        [SerializeField]
        [Tooltip("Hide the menu when the left hand is selecting an interaction.")]
        private bool hideMenuOnSelect = true;

        [SerializeField]
        [Tooltip("If true, palm up toggles the menu to a fixed position facing the user (show on first, hide on second); if false, menu follows palm up/down.")]
        private bool palmToggle = false;

        [SerializeField]
        [Tooltip("Follow preset for hand tracking mode.")]
        private FollowPresetDatumProperty handTrackingFollowPreset;

        [SerializeField]
        [Tooltip("Follow preset for controller mode.")]
        private FollowPresetDatumProperty controllerFollowPreset;

        [SerializeField]
        [Tooltip("Smoothing factor for menu position (higher = faster follow, 0-20).")]
        private float positionSmoothing = 10f;

        [SerializeField]
        [Tooltip("Smoothing factor for menu rotation (higher = faster follow, 0-20).")]
        private float rotationSmoothing = 10f;

        [SerializeField]
        [Tooltip("Low-pass filter strength for position smoothing (0-1, lower = more smoothing to reduce jitter).")]
        private float positionJitterSmoothing = 0.5f;

        [SerializeField]
        [Tooltip("Low-pass filter strength for rotation smoothing (0-1, lower = more smoothing to reduce jitter).")]
        private float rotationJitterSmoothing = 0.2f;

        private Transform offsetRoot;
        private Transform dummyRightOffset; // Workaround for ApplyPreset requiring right offset
        private XRInputModalityManager.InputMode currentInputMode = XRInputModalityManager.InputMode.None;
        private UIDocument uiDocument;
        private Vector3 hiddenPosition = Vector3.one * 100f; // Closer position to hide UI
        private Vector3 filteredPosition;
        private Quaternion filteredRotation;
        private bool menuVisibleState = false;
        private bool lastPalmFacingUp = false;
        private Vector3 fixedPosition;
        private Quaternion fixedRotation;

        protected void Awake()
        {
            // Validate inputs
            if (leftPalmAnchor == null)
            {
                enabled = false;
                return;
            }

            if (handMenuUIGameObject == null)
            {
                enabled = false;
                return;
            }

            // Get UIDocument
            uiDocument = handMenuUIGameObject.GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                enabled = false;
                return;
            }

            if (handTrackingFollowPreset == null || controllerFollowPreset == null)
            {
                enabled = false;
                return;
            }

            // Initialize offset root
            offsetRoot = new GameObject("Left Offset Root").transform;
            offsetRoot.SetParent(leftPalmAnchor);
            offsetRoot.localPosition = menuOffset;
            offsetRoot.localRotation = Quaternion.identity;

            // Create dummy right offset for ApplyPreset
            dummyRightOffset = new GameObject("Dummy Right Offset").transform;

            // Initialize UI position and filters
            handMenuUIGameObject.transform.position = hiddenPosition;
            filteredPosition = hiddenPosition;
            filteredRotation = Quaternion.identity;
            fixedPosition = hiddenPosition;
            fixedRotation = Quaternion.identity;
        }

        protected void OnEnable()
        {
            XRInputModalityManager.currentInputMode.SubscribeAndUpdate(OnInputModeChanged);
        }

        protected void OnDisable()
        {
            XRInputModalityManager.currentInputMode.Unsubscribe(OnInputModeChanged);
            if (uiDocument != null)
            {
                handMenuUIGameObject.transform.position = hiddenPosition;
            }
        }

        protected void OnDestroy()
        {
            if (offsetRoot != null) Destroy(offsetRoot.gameObject);
            if (dummyRightOffset != null) Destroy(dummyRightOffset.gameObject);
        }

        protected void OnValidate()
        {
            if (offsetRoot != null)
            {
                offsetRoot.localPosition = menuOffset;
                offsetRoot.localRotation = Quaternion.identity;
            }
            positionSmoothing = Mathf.Clamp(positionSmoothing, 0f, 20f);
            rotationSmoothing = Mathf.Clamp(rotationSmoothing, 0f, 20f);
            positionJitterSmoothing = Mathf.Clamp(positionJitterSmoothing, 0f, 1f);
            rotationJitterSmoothing = Mathf.Clamp(rotationJitterSmoothing, 0f, 1f);
        }

        private void OnInputModeChanged(XRInputModalityManager.InputMode newMode)
        {
            currentInputMode = newMode;
            var preset = GetCurrentPreset();
            if (preset != null && offsetRoot != null && dummyRightOffset != null)
            {
                preset.ApplyPreset(offsetRoot, dummyRightOffset);
                // Override preset offset with menuOffset
                offsetRoot.localPosition = menuOffset;
                offsetRoot.localRotation = Quaternion.identity;
            }
        }

        private FollowPreset GetCurrentPreset()
        {
            return currentInputMode == XRInputModalityManager.InputMode.MotionController
                ? controllerFollowPreset?.Value
                : handTrackingFollowPreset?.Value;
        }

        protected void LateUpdate()
        {
            // Check for valid inputs
            if (leftPalmAnchor == null || handMenuUIGameObject == null || uiDocument == null)
            {
                handMenuUIGameObject.transform.position = hiddenPosition;
                return;
            }

            var preset = GetCurrentPreset();
            if (preset == null)
            {
                handMenuUIGameObject.transform.position = hiddenPosition;
                return;
            }

            var cameraTransform = Camera.main?.transform;
            if (cameraTransform == null)
            {
                handMenuUIGameObject.transform.position = hiddenPosition;
                return;
            }

            // Check if hand is selecting
            bool isSelecting = false;
            if (hideMenuOnSelect)
            {
                var manager = Object.FindFirstObjectByType<XRInteractionManager>();
                if (manager != null)
                {
                    isSelecting = manager.IsHandSelecting(InteractorHandedness.Left);
                }
            }

            // Check palm orientation
            bool palmFacingUp = !isSelecting && IsPalmFacingUp(leftPalmAnchor, preset);

            // Toggle logic if palmToggle is true
            var referenceUp = Vector3.up;
            if (palmToggle)
            {
                if (palmFacingUp && !lastPalmFacingUp)
                {
                    menuVisibleState = !menuVisibleState;
                    if (menuVisibleState)
                    {
                        // Capture fixed position and rotation facing the camera
                        fixedPosition = offsetRoot.position;
                        filteredPosition = fixedPosition;
                        Vector3 cameraToUI = cameraTransform.position - fixedPosition;
                        fixedRotation = Quaternion.LookRotation(-cameraToUI, referenceUp);
                        filteredRotation = fixedRotation;
                    }
                }
            }
            else
            {
                menuVisibleState = palmFacingUp;
            }

            lastPalmFacingUp = palmFacingUp;

            // Update visibility
            SetMenuVisible(menuVisibleState);

            // Update position and rotation if visible and not in toggle mode
            if (!menuVisibleState)
            {
                return;
            }

            if (!palmToggle)
            {
                var targetPos = offsetRoot.position;
                // Apply low-pass filter for position jitter
                filteredPosition = Vector3.Lerp(filteredPosition, targetPos, positionJitterSmoothing);

                var targetRot = offsetRoot.rotation;
                // Apply low-pass filter for rotation jitter
                filteredRotation = Quaternion.Slerp(filteredRotation, targetRot, rotationJitterSmoothing);

                // Apply gaze snapping if enabled
                if (preset.snapToGaze)
                {
                    var gazeToObject = (offsetRoot.position - cameraTransform.position).normalized;
                    var objectToGaze = -gazeToObject;
                    var referenceAxis = preset.GetReferenceAxisForTrackingAnchor(leftPalmAnchor, false);
                    if (Vector3.Dot(referenceAxis, objectToGaze) > preset.snapToGazeDotThreshold)
                    {
                        BurstMathUtility.OrthogonalLookRotation(in gazeToObject, in referenceUp, out filteredRotation);
                    }
                }
            }

            // Smoothly update position and rotation
            handMenuUIGameObject.transform.position = Vector3.Lerp(
                handMenuUIGameObject.transform.position,
                palmToggle ? fixedPosition : filteredPosition,
                positionSmoothing * Time.deltaTime
            );
            handMenuUIGameObject.transform.rotation = Quaternion.Slerp(
                handMenuUIGameObject.transform.rotation,
                palmToggle ? fixedRotation : filteredRotation,
                rotationSmoothing * Time.deltaTime
            );

            // Debug visualization
            Debug.DrawRay(leftPalmAnchor.position, leftPalmAnchor.up * 0.1f, Color.green, 0.1f);
            Debug.DrawRay(palmToggle ? fixedPosition : filteredPosition, referenceUp * 0.1f, Color.blue, 0.1f);
        }

        private bool IsPalmFacingUp(Transform palmAnchor, FollowPreset preset)
        {
            if (palmAnchor == null || preset == null)
            {
                return false;
            }

            var referenceAxis = preset.GetReferenceAxisForTrackingAnchor(palmAnchor, false);
            var dotProduct = Vector3.Dot(referenceAxis, Vector3.up);
            return !preset.requirePalmFacingUp || dotProduct > preset.palmFacingUpDotThreshold;
        }

        private void SetMenuVisible(bool visible)
        {
            if (uiDocument == null)
            {
                return;
            }

            var targetPosition = visible ? (palmToggle ? fixedPosition : filteredPosition) : hiddenPosition;
            handMenuUIGameObject.transform.position = targetPosition;
            handMenuUIGameObject.transform.rotation = visible ? (palmToggle ? fixedRotation : filteredRotation) : Quaternion.identity;
        }
    }
}