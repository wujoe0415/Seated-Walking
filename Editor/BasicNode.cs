using UnityEditor.Experimental.GraphView;

namespace LocomotionStateMachine
{
    public class BasicNode : Node
    {
        public enum NodeType
        {
            Condition,
            Locomotion,
            Transition
        }
        public string GUID;
        public bool EntyPoint = false;
        public NodeType Type = NodeType.Locomotion;
    }
}