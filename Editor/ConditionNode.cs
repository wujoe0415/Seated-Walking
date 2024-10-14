using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.Port;

namespace LocomotionStateMachine
{

    public class ConditionNode : BasicNode
    {
        public StateCondition Condition;

        public ConditionNode(StateCondition condition, bool isEntry)
        {
            base.title = "Any";
            base.GUID = Guid.NewGuid().ToString();
            base.EntyPoint = isEntry;
            base.Type = NodeType.Condition;
            Condition = condition;

            var outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            var portLabel = outputPort.contentContainer.Q<Label>("type");
            outputPort.contentContainer.Remove(portLabel);

            outputPort.contentContainer.Add(new Label("  "));
            outputPort.portName = "output";
            outputContainer.Add(outputPort);

            RefreshPorts();
            RefreshExpandedState();
            styleSheets.Add(Resources.Load<StyleSheet>("ConditionNode"));
        }
    }
}
