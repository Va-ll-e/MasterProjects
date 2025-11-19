
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.Subsystems;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // URP Post-Processing

public class PinchLightController : MonoBehaviour
{
    [SerializeField] private Volume volume; // Assign Volume component (on Main Camera)
    [SerializeField] private GameObject gazeTarget; // Assign Sphere
    private HandsAggregatorSubsystem _aggregator;
    private GazeInteractor _gazeInteractor;
    private StatefulInteractable _targetInteractable;
    private Bloom _bloom;

    private void Start()
    {
        // Get the aggregator for hand data
        _aggregator = XRSubsystemHelpers.GetFirstSubsystem<HandsAggregatorSubsystem>();
        if (_aggregator == null)
        {
            Debug.LogError("No HandsAggregatorSubsystem found—check MRTK setup.");
            return;
        }

        // Get GazeInteractor
        _gazeInteractor = FindObjectOfType<GazeInteractor>();
        if (_gazeInteractor == null)
        {
            Debug.LogError("No GazeInteractor found—ensure XR Rig has Gaze Interactor.");
            return;
        }

        // Get StatefulInteractable from gazeTarget
        if (gazeTarget != null)
        {
            _targetInteractable = gazeTarget.GetComponent<StatefulInteractable>();
            if (_targetInteractable == null)
            {
                Debug.LogError("No StatefulInteractable on gazeTarget—add to sphere.");
            }
        }
        else
        {
            Debug.LogError("gazeTarget not assigned—assign sphere in Inspector.");
        }

        // Get Bloom from VolumeProfile
        if (volume != null && volume.profile != null)
        {
            if (!volume.profile.TryGet(out _bloom))
            {
                Debug.LogError("Bloom effect not found in Volume profile—add Bloom.");
            }
        }
        else
        {
            Debug.LogError("Volume or profile not assigned—check Inspector.");
        }
    }

    private void Update()
    {
        if (_aggregator.Equals(null)  || _bloom.Equals(null)  || _gazeInteractor.Equals(null)  || _targetInteractable.Equals(null) )
        { }
        else
        {
            var isGazing = _gazeInteractor.interactablesHovered.Contains(_targetInteractable);
            
            if (_aggregator.TryGetPinchProgress(XRNode.LeftHand, out bool isReadyToPinch, out bool isPinching, out float pinchAmount))
            {
                if (isReadyToPinch && isGazing)
                {
                    _bloom.intensity.value = (2f * pinchAmount) + .25f;
                }
                else if (isPinching)
                {
                    _bloom.intensity.value = (2f * pinchAmount);
                }
                else
                {
                    _bloom.intensity.value = 0f; // No glow
                }
            }
            // Check if gazing at the sphere
            else if (isGazing)
            {
                _bloom.intensity.value = .25f;
            }
            else
            {
                _bloom.intensity.value = 0f;
            }
        }
    }
}