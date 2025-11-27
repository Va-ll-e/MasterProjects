using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Control
{
    public class ExecuteMotionPrimitiveSequenceActionResult : ActionResult<ExecuteMotionPrimitiveSequenceResult>
    {
        public const string k_RosMessageName = "control_msgs/ExecuteMotionPrimitiveSequenceActionResult";
        public override string RosMessageName => k_RosMessageName;


        public ExecuteMotionPrimitiveSequenceActionResult() : base()
        {
            this.result = new ExecuteMotionPrimitiveSequenceResult();
        }

        public ExecuteMotionPrimitiveSequenceActionResult(HeaderMsg header, GoalStatusMsg status, ExecuteMotionPrimitiveSequenceResult result) : base(header, status)
        {
            this.result = result;
        }
        public static ExecuteMotionPrimitiveSequenceActionResult Deserialize(MessageDeserializer deserializer) => new ExecuteMotionPrimitiveSequenceActionResult(deserializer);

        ExecuteMotionPrimitiveSequenceActionResult(MessageDeserializer deserializer) : base(deserializer)
        {
            this.result = ExecuteMotionPrimitiveSequenceResult.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.status);
            serializer.Write(this.result);
        }


#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}
