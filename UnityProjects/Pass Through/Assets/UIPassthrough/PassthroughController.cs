using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;

public class PassthroughController : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;

    private VisualElement _root;

    private void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        
        var passThroughToggle = _root.Q<Toggle>("PassThroughToggle");

        var arCamera = mainCamera.GetComponent<ARCameraManager>();
        
        
         passThroughToggle.RegisterCallback<ChangeEvent<bool>>((evt) =>
        {
            arCamera.enabled = evt.newValue;
        });
        
        
    }
}
