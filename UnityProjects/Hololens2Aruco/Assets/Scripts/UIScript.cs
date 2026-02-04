using UnityEngine;
using UnityEngine.UIElements;


public class UIScript : MonoBehaviour
{
    public GameObject arucoGameObject;
    private ArUcoTracking _arucoTracking;
    private VisualElement _root;

    private Slider _xPos, _yPos, _zPos;
    private Slider _xRot, _yRot, _zRot;

    private Toggle _toggleAruco;

    private MeshRenderer _arucoMesh;
    
    
    void OnEnable()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _arucoTracking = GetComponent<ArUcoTracking>();
        _toggleAruco = _root.Q<Toggle>("toggleAruco");
        
        _arucoMesh = arucoGameObject.GetComponent<MeshRenderer>();
        _toggleAruco.RegisterValueChangedCallback(evt =>
        {
            _arucoMesh.enabled = evt.newValue;
        });
        
        var scanBtn = _root.Q<Button>("scanBtn");
        
        _xPos = _root.Q<Slider>("xPos");
        if (_xPos == null) Debug.LogError("UI Error: Could not find slider named 'xpos'");
        _yPos = _root.Q<Slider>("yPos");
        _zPos = _root.Q<Slider>("zPos");
        
        _xRot = _root.Q<Slider>("xRot");
        _yRot = _root.Q<Slider>("yRot");
        _zRot = _root.Q<Slider>("zRot");
        
        RegisterSliderCallbacks();
        
        
#if ENABLE_WINMD_SUPPORT
        scanBtn.clicked += () => _arucoTracking.RunArUcoTracking();
        scanBtn.clicked += () => { Debug.Log("scanBtn clicked"); };
#endif
    }

    private void RegisterSliderCallbacks()
    {
        _xPos.RegisterValueChangedCallback(evt => UpdateOffset());
        _yPos.RegisterValueChangedCallback(evt => UpdateOffset());
        _zPos.RegisterValueChangedCallback(evt => UpdateOffset());
     
        _xRot.RegisterValueChangedCallback(evt => UpdateOffset());
        _yRot.RegisterValueChangedCallback(evt => UpdateOffset());
        _zRot.RegisterValueChangedCallback(evt => UpdateOffset());
    }

    private void UpdateOffset()
    {
        
        arucoGameObject.transform.localPosition = new Vector3(_xPos.value, _yPos.value, _zPos.value);
        arucoGameObject.transform.localEulerAngles = new Vector3(_xRot.value, _yRot.value, _zRot.value);
        
    }
    
}
