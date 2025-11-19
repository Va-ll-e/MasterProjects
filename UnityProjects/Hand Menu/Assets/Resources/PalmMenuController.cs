using UnityEngine;
using UnityEngine.XR;

public class PalmMenuController : MonoBehaviour
{
    public GameObject palmMenu; // Assign your PalmMenuPanel GameObject in Inspector
    private bool isMenuActive = false;

    void Start()
    {
        // Ensure palmMenu is assigned and initially disabled
        if (palmMenu == null)
        {
            Debug.LogError("PalmMenu GameObject is not assigned!");
            return;
        }
        palmMenu.SetActive(false);
    }

    void Update()
    {
        // Get left hand device
        InputDevice leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (leftHand.isValid)
        {
            // Try to get hand rotation
            if (leftHand.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion handRotation))
            {
                // Approximate palm direction (OpenXR uses forward vector for hand orientation)
                Vector3 palmForward = handRotation * Vector3.forward;
                // Direction from hand to camera (user's head)
                Vector3 toCamera = (Camera.main.transform.position - handRotation * Vector3.zero).normalized;

                // Check if palm is facing the user (dot product < -0.5 means palm roughly faces camera)
                float dotProduct = Vector3.Dot(palmForward, toCamera);
                bool isPalmFacingUser = dotProduct < -0.5f; // Adjust threshold if needed

                // Toggle menu
                if (isPalmFacingUser && !isMenuActive)
                {
                    palmMenu.SetActive(true);
                    isMenuActive = true;
                    Debug.Log("Menu activated: Palm facing user");
                }
                else if (!isPalmFacingUser && isMenuActive)
                {
                    palmMenu.SetActive(false);
                    isMenuActive = false;
                    Debug.Log("Menu deactivated: Palm not facing user");
                }
            }
            else
            {
                Debug.LogWarning("Could not retrieve left hand rotation");
            }
        }
        else
        {
            Debug.LogWarning("Left hand device not detected");
        }
    }
}