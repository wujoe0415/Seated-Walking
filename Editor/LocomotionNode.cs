using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;
using Unity.VisualScripting;

namespace LocomotionStateMachine
{
    public class LocomotionNode: BasicNode
    {
        public string StateName;
        public Action<Node, Port> OnDeletePort;
        public LocomotionNode(string title, string name, bool isEntry, bool inputOnly)
        {
            base.title = title;
            base.GUID = Guid.NewGuid().ToString();
            base.EntyPoint = isEntry;
            base.Type = NodeType.Locomotion;
            StateName = name;
            if (isEntry)
                return;
            var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(string));
            inputPort.portColor = new Color(0.3f, 0.7f, 0.4f, 0.9f);
            inputPort.portName = "Input";
            inputContainer.Add(inputPort);
                
            var textField = new TextField("");
            textField.RegisterValueChangedCallback(evt =>
            {
                StateName = evt.newValue;
                if (!inputOnly)
                    base.title = evt.newValue;
                else
                    base.title = $"Jump to {evt.newValue}";
            });
            textField.SetValueWithoutNotify(name);

            if (!inputOnly)
            {
                var button = new Button(() => { AddOutputPort(); })
                {
                    text = "Add New Connection"
                };
                titleButtonContainer.Add(button);
            }
            VisualElement main = new VisualElement();
            main.Add(textField);
            main.style.backgroundColor = new Color(0.14f, 0.14f, 0.14f, 0.9f);

            mainContainer.Add(main);
            
            styleSheets.Add(Resources.Load<StyleSheet>("Node"));
            
            //mainContainer.style.backgroundColor = new Color(1, 1, 0, 0.5f);
            //inputContainer.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            //outputContainer.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            RefreshExpandedState();
            RefreshPorts();
        }
        public void AddOutputPort(string overrideName = "")
        {
            var generatedPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            var portLabel = generatedPort.contentContainer.Q<Label>("type");
            generatedPort.contentContainer.Remove(portLabel);

            var outputPortCount = outputContainer.Query("connector").ToList().Count;
            var outputPortName = string.IsNullOrEmpty(overrideName)? $"Option {outputPortCount + 1}":overrideName;


            //var textField = new TextField()
            //{
            //    name = string.Empty,
            //    value = outputPortName
            //};
            //textField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
            generatedPort.contentContainer.Add(new Label("  "));
            //generatedPort.contentContainer.Add(textField);
            var deleteButton = new Button(() => { 
                   OnDeletePort?.Invoke(this, generatedPort);
                UpdateAllOutputPorts();
                })
            {
                text = "X"
            };
            generatedPort.contentContainer.Add(deleteButton);
            generatedPort.portName = outputPortName;
            outputContainer.Add(generatedPort);
            RefreshPorts();
            RefreshExpandedState();
        }
        public void UpdateAllOutputPorts()
        {
            var outputPorts = outputContainer.Query<Port>().ToList();

            for (int i = 1; i < outputPorts.Count; i++)
            {
                Port tmp = outputPorts[i];
                tmp.portName = $"Condition {i + 1}";
                tmp.MarkDirtyRepaint();  // Ensures that the UI refreshes with the updated name
            }

            RefreshExpandedState(); // Refreshes the node's layout to reflect name changes
            RefreshPorts(); // Ensures the ports are properly refreshed in the UI
        }
    }
}
