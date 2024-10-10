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
            base.title = "Condition";
            base.GUID = Guid.NewGuid().ToString();
            base.EntyPoint = isEntry;
            base.Type = NodeType.Condition;
            Condition = condition;

            //mainContainer.Add(new Label("Previous Movement"));
            //mainContainer.Add(CreateMovementEnumeratorUI(Condition.PreviousMovement));

            // UI for TriggerMovement (MovementEnumerator)
            mainContainer.Add(new Label("Trigger Movement"));
            mainContainer.Add(CreateMovementEnumeratorUI());

            // Dropdown for LastDeviceState
            EnumField lastStateField = new EnumField("Last Device State", Condition.LastDeviceState);
            lastStateField.RegisterValueChangedCallback(evt =>
            {
                Condition.LastDeviceState = (HistoryState)evt.newValue;
            });
            mainContainer.Add(lastStateField);

            // Dropdown for DeviceType
            EnumField deviceTypeField = new EnumField("Device Type", Condition.Device);
            deviceTypeField.RegisterValueChangedCallback(evt =>
            {
                Condition.Device = (DeviceType)evt.newValue;
            });
            mainContainer.Add(deviceTypeField);


            var outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            var portLabel = outputPort.contentContainer.Q<Label>("type");
            outputPort.contentContainer.Remove(portLabel);

            outputPort.contentContainer.Add(new Label("  "));
            outputPort.portName = "output";
            outputContainer.Add(outputPort);

            RefreshPorts();
            RefreshExpandedState();
        }
        private VisualElement CreateMovementEnumeratorUI()
        {
            // Container for MovementEnumerator
            VisualElement container = new VisualElement();
            container.style.paddingLeft = 10;

            // Add movement fields
            container.Add(CreateMovementField("Left Toe", Condition.TriggerMovement.LeftToe, newValue => Condition.TriggerMovement.LeftToe = newValue));
            container.Add(CreateMovementField("Left Heel", Condition.TriggerMovement.LeftHeel, newValue => Condition.TriggerMovement.LeftHeel = newValue));
            container.Add(CreateMovementField("Right Toe", Condition.TriggerMovement.RightToe, newValue => Condition.TriggerMovement.RightToe = newValue));
            container.Add(CreateMovementField("Right Heel", Condition.TriggerMovement.RightHeel, newValue => Condition.TriggerMovement.RightHeel = newValue));

            return container;
        }
        private VisualElement CreateMovementField(string label, Movement currentValue, System.Action<Movement> onValueChanged)
        {
            VisualElement fieldContainer = new VisualElement();
            fieldContainer.style.flexDirection = FlexDirection.Row;

            Label fieldLabel = new Label(label);
            EnumFlagsField movementField = new EnumFlagsField(currentValue);
            movementField.RegisterValueChangedCallback(evt =>
            {
                onValueChanged((Movement)evt.newValue);
            });

            fieldContainer.Add(fieldLabel);
            fieldContainer.Add(movementField);

            return fieldContainer;
        }
    }
}
