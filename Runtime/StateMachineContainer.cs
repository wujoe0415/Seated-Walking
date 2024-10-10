using System;
using System.Collections.Generic;
using UnityEngine;

namespace LocomotionStateMachine
{
    public class StateMachineContainer : ScriptableObject
    {
        public List<NodeLinkData> NodeLinks = new List<NodeLinkData>();
        public List<LocomotionNodeData> LocomotionNodeData = new List<LocomotionNodeData>();
        // TODO: Save and Load Implementation
        public List<TransitionNodeData> TransitionNodeData = new List<TransitionNodeData>();
        public List<ConditionNodeData> ConditionNodeData = new List<ConditionNodeData>();
        public List<ExposedProperty> ExposedProperties = new List<ExposedProperty>();
        public List<BlockData> BlockData = new List<BlockData>();
    }
}
