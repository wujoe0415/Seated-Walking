using System;
using UnityEditor.Experimental.GraphView;
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

            mainContainer.Add(new Label("Previous Movement"));
            mainContainer.Add(CreateMovementEnumeratorUI(Condition.PreviousMovement));

            // UI for TriggerMovement (MovementEnumerator)
            mainContainer.Add(new Label("Trigger Movement"));
            mainContainer.Add(CreateMovementEnumeratorUI(Condition.TriggerMovement));

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
        private VisualElement CreateMovementEnumeratorUI(MovementEnumerator movementEnumerator)
        {
            // Container for MovementEnumerator
            VisualElement container = new VisualElement();
            container.style.paddingLeft = 10;

            // Add movement fields
            container.Add(CreateMovementField("Left Toe", movementEnumerator.LeftToe, newValue => movementEnumerator.LeftToe |= newValue));
            container.Add(CreateMovementField("Left Heel", movementEnumerator.LeftHeel, newValue => movementEnumerator.LeftHeel |= newValue));
            container.Add(CreateMovementField("Right Toe", movementEnumerator.RightToe, newValue => movementEnumerator.RightToe |= newValue));
            container.Add(CreateMovementField("Right Heel", movementEnumerator.RightHeel, newValue => movementEnumerator.RightHeel |= newValue));

            return container;
        }
        private VisualElement CreateMovementField(string label, Movement currentValue, System.Action<Movement> onValueChanged)
        {
            VisualElement fieldContainer = new VisualElement();
            fieldContainer.style.flexDirection = FlexDirection.Row;

            Label fieldLabel = new Label(label);
            EnumField movementField = new EnumField(currentValue);
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
