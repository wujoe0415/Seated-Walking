

using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.Port;
namespace LocomotionStateMachine
{
    public class TransitionNode : BasicNode
    {
        public StateTransition.BooleanOperator Operator;
        public Action<Node, Port> OnDeletePort;

        public TransitionNode(StateTransition.BooleanOperator transitionOperator, bool isEntry)
        {
            base.title = "Transition";
            base.GUID = Guid.NewGuid().ToString();
            base.EntyPoint = isEntry;
            base.Type = NodeType.Transition;

            Operator = transitionOperator;
            // Add input port to accept StateConditionNode connections
            Port inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            inputPort.portName = "Input State";
            inputContainer.Add(inputPort);


            Port outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(string));
            outputPort.portName = "Output State"; 
            outputPort.portColor = new Color(0.3f, 0.7f, 0.4f, 0.9f);

            outputContainer.Add(outputPort);

            // Dropdown for BooleanOperator selection
            VisualElement main = new VisualElement();
            EnumField operatorField = new EnumField("Operator", Operator);
            operatorField.RegisterValueChangedCallback(evt =>
            {
                Operator = (StateTransition.BooleanOperator)evt.newValue;
            });
            main.Add(operatorField);
            main.style.backgroundColor = new Color(0.14f, 0.14f, 0.14f, 0.9f);

            mainContainer.Add(main);
            var button = new Button(() => { AddInputPort(); })
            {
                text = "Add Condition"
            };
            this.titleButtonContainer.Add(button);

            RefreshExpandedState();
            RefreshPorts();
            styleSheets.Add(Resources.Load<StyleSheet>("TransitionNode"));
        }
        public void AddInputPort(string overrideName = "")
        {
            Port generatedPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Capacity.Single, typeof(bool));
            generatedPort.contentContainer.Add(new Label("  "));
            //generatedPort.contentContainer.Add(textField);
            int inputPortCount = inputContainer.Query("connector").ToList().Count;
            var inputPortName = string.IsNullOrEmpty(overrideName)? $"Condition {inputPortCount + 1}": overrideName;

            var deleteButton = new Button(() => { 
                OnDeletePort?.Invoke(this, generatedPort);
                UpdateAllConditionPorts();
            })
            {
                text = "X"
            };
            generatedPort.contentContainer.Add(deleteButton);
            generatedPort.portName = inputPortName;
            inputContainer.Add(generatedPort);
            RefreshPorts();
            RefreshExpandedState();
        }
        public void UpdateAllConditionPorts()
        {
            var inputPorts = inputContainer.Query<Port>().ToList();

            for (int i = 1; i < inputPorts.Count; i++)
            {
                Port tmp = inputPorts[i];
                tmp.portName = $"Condition {i + 1}";
                tmp.MarkDirtyRepaint();  // Ensures that the UI refreshes with the updated name
            }

            RefreshExpandedState(); // Refreshes the node's layout to reflect name changes
            RefreshPorts(); // Ensures the ports are properly refreshed in the UI
        }
    }
    public class StateEdgeConnectorListener : IEdgeConnectorListener
    {
        private readonly Action<Edge> onEdgeConnected;
        private readonly Action<Edge> onEdgeDisconnected;

        public StateEdgeConnectorListener(Action<Edge> onEdgeConnected, Action<Edge> onEdgeDisconnected)
        {
            this.onEdgeConnected = onEdgeConnected;
            this.onEdgeDisconnected = onEdgeDisconnected;
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position) { }

        public void OnDrop(GraphView graphView, Edge edge)
        {
            if (edge.input != null && edge.output != null)
            {
                // Call the appropriate action when the edge is connected
                onEdgeConnected?.Invoke(edge);
                graphView.AddElement(edge);
            }
            else
            {
                // Call the appropriate action when the edge is disconnected
                onEdgeDisconnected?.Invoke(edge);
                edge.input?.Disconnect(edge);
                edge.output?.Disconnect(edge);
            }
        }
    }
}
