using System;
using RosMessageTypes.ControllerManager;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

namespace URDF.Research_3.Scripts
{
    public class EmergencyStop : MonoBehaviour
    {
        private ROSConnection _ros;
        private BoolMsg _stopMessage;

        private void Start()
        {
            _ros = GetComponent<ROSConnection>();
            _ros.RegisterPublisher<BoolMsg>("/emergency_stop");
            _stopMessage = new BoolMsg
            {
                data = true
            };
        }
        
        public void TriggerEmergencyStop()
        {
            _ros.Publish("/emergency_stop", _stopMessage);

            Debug.Log("Triggering Emergency Stop");
        }
    }
} 
/* Stopping all controllers FAILED
 {
    ROSConnection ros;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        // Pre-register services to avoid empty destination
        ros.RegisterRosService<ListControllersRequest, ListControllersResponse>("/controller_manager/list_controllers");
        ros.RegisterRosService<SwitchControllerRequest, SwitchControllerResponse>("/controller_manager/switch_controller");
    }

    public void TriggerEmergencyStop()
    {
        Debug.Log("=== EMERGENCY STOP TRIGGERED ===");

        // THIS IS THE ONLY WORKING CALL IN v0.7.0
        ros.SendServiceMessage<ListControllersResponse>(
            "/controller_manager/list_controllers",
            new ListControllersRequest(),
            OnListControllersResponse);
    }

    private void OnListControllersResponse(ListControllersResponse resp)
    {
        if (resp == null)
        {
            Debug.LogError("list_controllers returned null");
            return;
        }

        var toStop = resp.controller
            .Where(c => c.state == "active" && !c.name.Contains("joint_state_broadcaster"))
            .Select(c => c.name)
            .ToArray();

        if (toStop.Length == 0)
        {
            Debug.Log("No motion controllers active");
            return;
        }

        Debug.Log("Deactivating: " + string.Join(", ", toStop));

        var req = new SwitchControllerRequest
        {
            activate_controllers   = Array.Empty<string>(),
            deactivate_controllers = toStop,
            strictness             = SwitchControllerRequest.BEST_EFFORT,
            timeout                = new DurationMsg { sec = 3, nanosec = 0 }
        };

        ros.SendServiceMessage<SwitchControllerResponse>(
            "/controller_manager/switch_controller",
            req,
            OnSwitchResponse);
    }

    private void OnSwitchResponse(SwitchControllerResponse resp)
    {
        if (resp == null)
        {
            Debug.LogError("switch_controller returned null");
            return;
        }

        if (resp.ok)
            Debug.Log("EMERGENCY STOP SUCCESSFUL – Robot is completely stopped");
        else
            Debug.LogError("Switch failed: " + resp.message);
    }
}
*/

/* Sending a Zero velocity to topic /dynamic_joint_states FAILED 
 {
    private ROSConnection _ros;
    private Fr3Sync _robot;
    private JointStateMsg _stopMsg;
    
    void Start()
    {
        // EmergencyStop is located within the ROSController which also has a ROSConnection component
        _ros = GetComponent<ROSConnection>();
        
        _stopMsg = new JointStateMsg
        {
            header = GetRosHeader(),
            name = new[]
            {
                "fr3_finger_joint1","fr3_joint1","fr3_joint2","fr3_joint3","fr3_joint4",
                "fr3_joint5","fr3_joint6","fr3_joint7"
            },
            position = _robot.GetPositions(),   // current pose in radians
            velocity = new double[] { 0,0,0,0,0,0,0,0 }, // THIS STOPS THE ROBOT
            effort   = new double[8]
        };
    }

    public void TriggerEmergencyStop()
    {
        _ros.Publish("/dynamic_joint_states", _stopMsg);
        Debug.Log("EMERGENCY STOP SENT – zero velocity published");
    }
    
    private HeaderMsg GetRosHeader()
    {
        var now = System.DateTime.UtcNow;

        var seconds = now.Ticks / System.TimeSpan.TicksPerSecond;
        var nanos = (now.Ticks % System.TimeSpan.TicksPerSecond) * 100;

        return new HeaderMsg
        {
            stamp = new RosMessageTypes.BuiltinInterfaces.TimeMsg
            {
                sec = (int)seconds,
                nanosec = (uint)nanos
            },
            frame_id = "base_link"
        };
    }
    }
*/


/* Trying to zero velocity to topic /joint_positions
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

namespace URDF.Research_3.Scripts
{
    public class EmergencyStop : MonoBehaviour
    {
        private ROSConnection _ros;
        private JointStateMsg _stopMessage;
        private Fr3Sync _fr3State;

        private void Start()
        {
            _ros = GetComponent<ROSConnection>();
            _ros.RegisterPublisher<JointStateMsg>("/joint_states");
            _stopMessage = new JointStateMsg
            {
                header = GetRosHeader(),
                name = new string[]
                {
                    "fr3_finger_joint1",
                    "fr3_joint1", 
                    "fr3_joint2", 
                    "fr3_joint3", 
                    "fr3_joint4", 
                    "fr3_joint5", 
                    "fr3_joint6", 
                    "fr3_joint7"
                },
                position = new double[8],
                velocity = new double[8],
                effort = new double[8]
            };
            
            // Getting the _fr3State used to get the exact robot position when it sends an emergency stop 
            _fr3State = GameObject.Find("fr3").GetComponent<Fr3Sync>();
            
        }

        private HeaderMsg GetRosHeader()
        {
            var now = System.DateTime.UtcNow;

            var seconds = now.Ticks / System.TimeSpan.TicksPerSecond;
            var nanos = (now.Ticks % System.TimeSpan.TicksPerSecond) * 100;

            return new HeaderMsg
            {
                stamp = new RosMessageTypes.BuiltinInterfaces.TimeMsg
                {
                    sec = (int)seconds,
                    nanosec = (uint)nanos
                },
                frame_id = "base_link"
            };
        }
        
        public void TriggerEmergencyStop()
        {
            Debug.Log("Hello from TriggerEmergencyStop");
            _stopMessage.position = _fr3State.GetLinkPositions();
            
            _ros.Publish("/joint_states", _stopMessage);
        }
    }
} 
 */