using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.Control
{
    public class ExecuteMotionPrimitiveSequenceActionGoal : ActionGoal<ExecuteMotionPrimitiveSequenceGoal>
    {
        public const string k_RosMessageName = "control_msgs/ExecuteMotionPrimitiveSequenceActionGoal";
        public override string RosMessageName => k_RosMessageName;


        public ExecuteMotionPrimitiveSequenceActionGoal() : base()
        {
            this.goal = new ExecuteMotionPrimitiveSequenceGoal();
        }

        public ExecuteMotionPrimitiveSequenceActionGoal(HeaderMsg header, GoalIDMsg goal_id, ExecuteMotionPrimitiveSequenceGoal goal) : base(header, goal_id)
        {
            this.goal = goal;
        }
        public static ExecuteMotionPrimitiveSequenceActionGoal Deserialize(MessageDeserializer deserializer) => new ExecuteMotionPrimitiveSequenceActionGoal(deserializer);

        ExecuteMotionPrimitiveSequenceActionGoal(MessageDeserializer deserializer) : base(deserializer)
        {
            this.goal = ExecuteMotionPrimitiveSequenceGoal.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.goal_id);
            serializer.Write(this.goal);
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
