using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuController : MonoBehaviour
{
	private VisualElement _buttonsWrapper;

	private Button _colorUiButton;
	private Button _shapeUiButton;
	private Button _controlUiButton;
	
    [SerializeField] private VisualTreeAsset colorButtonTemplate;
    private VisualElement _colorButtons;
	
    [SerializeField] private VisualTreeAsset shapeButtonTemplate;
    private VisualElement _shapeButtons;
	
    [SerializeField] private VisualTreeAsset controlButtonTemplate;
    private VisualElement _controlButtons;
    
    [SerializeField] private GameObject entity;

    private void Awake()
    {
	    var root = GetComponent<UIDocument>().rootVisualElement;
	    
	    _buttonsWrapper = root.Q<VisualElement>("Buttons");
	    
	    _colorButtons = colorButtonTemplate.CloneTree();
	    _shapeButtons = shapeButtonTemplate.CloneTree();
	    _controlButtons = controlButtonTemplate.CloneTree();
	    
	    
	    // Buttons and functions related to the Main UI
	    _colorUiButton = root.Q<Button>("ColorButton");
	    _colorUiButton.clicked += ColorButtonOnClicked;
	    
	    _shapeUiButton = root.Q<Button>("ShapeButton");
	    _shapeUiButton.clicked += ShapeButtonOnClicked;
	    
	    _controlUiButton = root.Q<Button>("ControlButton");
	    _controlUiButton.clicked += ControlButtonOnClicked;
	    
	    
	    // Buttons and functions related to the Color UI
	    var colorBackButton = _colorButtons.Q<Button>("BackButton");
	    var redBtn = _colorButtons.Q<Button>("RedButton");
	    var greenBtn = _colorButtons.Q<Button>("GreenButton");
	    var blueBtn = _colorButtons.Q<Button>("BlueButton");
	    
	    colorBackButton.clicked += BackButtonOnClicked;
	    redBtn.clicked += () => SetColor(Color.red);
	    greenBtn.clicked += () => SetColor(Color.green);
	    blueBtn.clicked += () => SetColor(Color.blue);
	    
	    
	    // Buttons and functions related to the Shape UI
	    var shapeBackButton = _shapeButtons.Q<Button>("BackButton");
	    var cubeBtn = _shapeButtons.Q<Button>("CubeButton");
	    var sphereBtn = _shapeButtons.Q<Button>("SphereButton");
	    
	    shapeBackButton.clicked += BackButtonOnClicked;
	    cubeBtn.clicked += () => SetShape(Resources.GetBuiltinResource<Mesh>("Cube.fbx"));
	    sphereBtn.clicked += () => SetShape(Resources.GetBuiltinResource<Mesh>("Sphere.fbx"));
	    
	    
	    // Buttons and functions related to the Control UI
	    var controlBackButton = _controlButtons.Q<Button>("BackButton");
	    var resetBtn = _controlButtons.Q<Button>("ResetButton");
	    var jumpBtn = _controlButtons.Q<Button>("JumpButton");
	    
	    controlBackButton.clicked += BackButtonOnClicked;
	    resetBtn.clicked += ResetEntityPosition;
	    jumpBtn.clicked += Jump;
	    
    }


    private void Jump()
    {
	    entity.transform.position += Vector3.up; 
    }
    private void Flip(){}

    private void ResetEntityPosition()
    {
	    entity.transform.position = new Vector3(1f, 1f, 2f);
    }
	    
    
    private void SetShape(Mesh m)
    {
	    entity.GetComponent<MeshFilter>().mesh = m;
    }
    
    private void SetColor(Color c)
    {
	    entity.GetComponent<Renderer>().material.color = c;
    }
    
    private void BackButtonOnClicked()
    {
	 _buttonsWrapper.Clear();
	 _buttonsWrapper.Add(_colorUiButton);
	 _buttonsWrapper.Add(_shapeUiButton);
	 _buttonsWrapper.Add(_controlUiButton);
    }

    private void ColorButtonOnClicked()
    {
	    _buttonsWrapper.Clear();
	    _buttonsWrapper.Add(_colorButtons);
    }

    private void ShapeButtonOnClicked()
    {
	    _buttonsWrapper.Clear();
	    _buttonsWrapper.Add(_shapeButtons);
    }

    private void ControlButtonOnClicked()
    {
	    _buttonsWrapper.Clear();
	    _buttonsWrapper.Add(_controlButtons);
    }
    

}

