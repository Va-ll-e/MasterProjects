using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;


namespace RosMessageTypes.Control
{
    public class ExecuteMotionPrimitiveSequenceAction : Action<ExecuteMotionPrimitiveSequenceActionGoal, ExecuteMotionPrimitiveSequenceActionResult, ExecuteMotionPrimitiveSequenceActionFeedback, ExecuteMotionPrimitiveSequenceGoal, ExecuteMotionPrimitiveSequenceResult, ExecuteMotionPrimitiveSequenceFeedback>
    {
        public const string k_RosMessageName = "control_msgs/ExecuteMotionPrimitiveSequenceAction";
        public override string RosMessageName => k_RosMessageName;


        public ExecuteMotionPrimitiveSequenceAction() : base()
        {
            this.action_goal = new ExecuteMotionPrimitiveSequenceActionGoal();
            this.action_result = new ExecuteMotionPrimitiveSequenceActionResult();
            this.action_feedback = new ExecuteMotionPrimitiveSequenceActionFeedback();
        }

        public static ExecuteMotionPrimitiveSequenceAction Deserialize(MessageDeserializer deserializer) => new ExecuteMotionPrimitiveSequenceAction(deserializer);

        ExecuteMotionPrimitiveSequenceAction(MessageDeserializer deserializer)
        {
            this.action_goal = ExecuteMotionPrimitiveSequenceActionGoal.Deserialize(deserializer);
            this.action_result = ExecuteMotionPrimitiveSequenceActionResult.Deserialize(deserializer);
            this.action_feedback = ExecuteMotionPrimitiveSequenceActionFeedback.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.action_goal);
            serializer.Write(this.action_result);
            serializer.Write(this.action_feedback);
        }

    }
}
