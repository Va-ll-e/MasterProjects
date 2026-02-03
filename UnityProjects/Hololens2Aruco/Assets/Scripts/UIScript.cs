using UnityEngine;
using UnityEngine.UIElements;


public class UIScript : MonoBehaviour
{
    public GameObject arucoGameObject;
    private ArUcoTracking _arucoTracking;
    private VisualElement _root;

    private Slider _xPos, _yPos, _zPos;
    private Slider _xRot, _yRot, _zRot;

    
    
    void OnEnable()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _arucoTracking = GetComponent<ArUcoTracking>();
        var scanBtn = _root.Q<Button>("scanBtn");
        
#if ENABLE_WINMD_SUPPORT
        scanBtn.clicked += () => _arucoTracking.RunArUcoTracking();
        scanBtn.clicked += () => { Debug.Log("scanBtn clicked"); };
#endif
    }
    
}
