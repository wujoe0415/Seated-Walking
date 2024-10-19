using System;
using System.Collections.Generic;
using UnityEngine;

namespace LocomotionStateMachine
{
    [Serializable]
    [CreateAssetMenu(menuName = "Graph/State Machine")]
    public class StateMachineContainer : ScriptableObject
    {
        public List<NodeLinkData> NodeLinks = new List<NodeLinkData>();
        public List<LocomotionNodeData> LocomotionNodeData = new List<LocomotionNodeData>();
        // TODO: Save and Load Implementation
        public List<TransitionNodeData> TransitionNodeData = new List<TransitionNodeData>();
        public List<ConditionNodeData> ConditionNodeData = new List<ConditionNodeData>();
        public List<GroupData> GroupDatas = new List<GroupData>();
        public List<ExposedProperty> ExposedProperties = new List<ExposedProperty>();
    }
}
