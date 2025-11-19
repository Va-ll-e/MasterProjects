
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.UIElements;



public class TurtleController : MonoBehaviour
{

    [SerializeField] private ROSConnection ros;
    private Button forwardBtn, backBtn,  leftBtn, rightBtn;
    private float speed = 1.0f;
    private float turnSpeed = 1.0f;
    
    
    void Start()
    {
        ros.RegisterPublisher<TwistMsg>("/turtle1/cmd_vel");
        
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        forwardBtn = root.Q<Button>("ForwardBTN");
        backBtn = root.Q<Button>("BackBTN");
        leftBtn = root.Q<Button>("LeftBTN");
        rightBtn = root.Q<Button>("RightBTN");
        
        forwardBtn.clicked += () => PublishTwist(speed, 0f);
        forwardBtn.clicked += () => Debug.Log("Forward");
        
        backBtn.clicked += () => PublishTwist(-speed, 0f);
        backBtn.clicked += () => Debug.Log("Back");
        
        leftBtn.clicked += () => PublishTwist(0f, turnSpeed);
        leftBtn.clicked += () => Debug.Log("Left");
        
        rightBtn.clicked += () => PublishTwist(0f, -turnSpeed);
        rightBtn.clicked += () => Debug.Log("Right");
        
    }

    private void PublishTwist(float linearX, float angularZ)
    {
        if (ros.HasConnectionError) return;

        var twist = new TwistMsg
        {
            linear = new Vector3Msg(linearX, 0.0f, 0.0f),
            angular = new Vector3Msg(0.0f, 0.0f, angularZ),
        };

        ros.Publish("/turtle1/cmd_vel", twist);
        Debug.Log($"Published twist: {twist} with linear.x={linearX}, angular.z={angularZ}");

    }
}
