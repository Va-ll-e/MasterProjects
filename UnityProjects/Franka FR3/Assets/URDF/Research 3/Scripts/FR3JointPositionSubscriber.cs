using System;
using System.Collections.Generic;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.UIElements;

namespace URDF.Research_3.Scripts
{
    public class Fr3Sync : MonoBehaviour
    {
        [SerializeField] private ROSConnection ros;
        [SerializeField] private UpdatePackagesReceived pr;

        private readonly Dictionary<string, ArticulationBody> _jointDict = new();
        
        void Start()
        {
            
            ros.Subscribe<JointStateMsg>("NS_1/joint_states_throttle", OnJointState);
            
            //Populate the Dictionary
            _jointDict.Add("fr3_joint1", GameObject.Find("fr3_link1").GetComponent<ArticulationBody>());
            _jointDict.Add("fr3_joint2", GameObject.Find("fr3_link2").GetComponent<ArticulationBody>());
            _jointDict.Add("fr3_joint3", GameObject.Find("fr3_link3").GetComponent<ArticulationBody>());
            _jointDict.Add("fr3_joint4", GameObject.Find("fr3_link4").GetComponent<ArticulationBody>());
            _jointDict.Add("fr3_joint5", GameObject.Find("fr3_link5").GetComponent<ArticulationBody>());
            _jointDict.Add("fr3_joint6", GameObject.Find("fr3_link6").GetComponent<ArticulationBody>());
            _jointDict.Add("fr3_joint7", GameObject.Find("fr3_link7").GetComponent<ArticulationBody>());
        }

        void OnJointState(JointStateMsg msg)
        {
            for (int i = 0; i < msg.name.Length; i++)
            {
                var jointName = msg.name[i];
                if (!jointName.StartsWith("fr3_joint")) continue;
                
                if (_jointDict.TryGetValue(jointName, out var joint))
                {
                    var drive = joint.xDrive;
                    // 57.29 is 180/PI rounded
                    var position = (float)(msg.position[i]*57.29);
                    drive.target = position;
                    joint.xDrive = drive;
                }
            }
            pr.UpdatePackagesReceivedText();
         }
    }
}
