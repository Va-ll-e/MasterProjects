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
        
        public void SendEmergencyStop()
        {
            Debug.Log("Hello from SendEmergencyStop");
            _stopMessage.position = _fr3State.GetLinkPositions();
            
            _ros.Publish("/joint_states", _stopMessage);
        }
    }
}
