using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System;
using UnityEngine.UIElements;

namespace LocomotionStateMachine
{

    public class JumpNode : BasicNode
    {
        public string StateName;

        public JumpNode(string stateName, string name, bool isEntry)
        {
            base.title = title;
            base.GUID = Guid.NewGuid().ToString();
            base.EntyPoint = isEntry;
            base.Type = NodeType.Jump;
            StateName = stateName;

            var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(string));
            inputPort.portName = "Input";
            inputContainer.Add(inputPort);

            var textField = new TextField("Jump to");
            textField.RegisterValueChangedCallback(evt =>
            {
                StateName = evt.newValue;
                base.title = evt.newValue;
            });
            textField.SetValueWithoutNotify(title);
            mainContainer.Add(textField);
            RefreshExpandedState();
            RefreshPorts();
        }
    }
}
