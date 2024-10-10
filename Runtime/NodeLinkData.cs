using System;

namespace LocomotionStateMachine
{
    [Serializable]
    public class NodeLinkData
    {
        public string BaseNodeGUID;
        public string BasePortName;
        public string TargetPortName;
        public string TargetNodeGUID;
    }
}