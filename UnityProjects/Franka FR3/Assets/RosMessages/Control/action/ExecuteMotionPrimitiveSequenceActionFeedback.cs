using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Control
{
    public class ExecuteMotionPrimitiveSequenceActionFeedback : ActionFeedback<ExecuteMotionPrimitiveSequenceFeedback>
    {
        public const string k_RosMessageName = "control_msgs/ExecuteMotionPrimitiveSequenceActionFeedback";
        public override string RosMessageName => k_RosMessageName;


        public ExecuteMotionPrimitiveSequenceActionFeedback() : base()
        {
            this.feedback = new ExecuteMotionPrimitiveSequenceFeedback();
        }

        public ExecuteMotionPrimitiveSequenceActionFeedback(HeaderMsg header, GoalStatusMsg status, ExecuteMotionPrimitiveSequenceFeedback feedback) : base(header, status)
        {
            this.feedback = feedback;
        }
        public static ExecuteMotionPrimitiveSequenceActionFeedback Deserialize(MessageDeserializer deserializer) => new ExecuteMotionPrimitiveSequenceActionFeedback(deserializer);

        ExecuteMotionPrimitiveSequenceActionFeedback(MessageDeserializer deserializer) : base(deserializer)
        {
            this.feedback = ExecuteMotionPrimitiveSequenceFeedback.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.status);
            serializer.Write(this.feedback);
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
