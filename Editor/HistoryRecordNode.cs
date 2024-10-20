using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace LocomotionStateMachine
{
    public class HistoryRecordNode : ConditionNode
    {
        public HistoryRecordNode(StateCondition condition, bool isEntry) : base(condition, isEntry)
        {
            base.title = "Last Device State";
            // Dropdown for LastDeviceState
            VisualElement upper = new VisualElement();
            upper.style.flexDirection = FlexDirection.Row;
            Label label = new Label("Condition");
            label.style.width = 50;
            Condition.LastDeviceState = condition.LastDeviceState == HistoryState.Any?HistoryState.LastSteppedToe : condition.LastDeviceState;
            EnumField lastStateField = new EnumField(Condition.LastDeviceState);
            lastStateField.RegisterValueChangedCallback(evt =>
            {
                Condition.LastDeviceState = (HistoryState)evt.newValue;
            });
            upper.Add(label);
            upper.Add(lastStateField);
            upper.style.width = 180;
            upper.style.backgroundColor = new Color(0.14f, 0.14f, 0.14f, 0.9f);
            mainContainer.Add(upper);
            // Dropdown for DeviceType
            VisualElement lower = new VisualElement();
            lower.style.flexDirection = FlexDirection.Row;
            Label deviceLabel = new Label("was");
            Condition.Device = condition.Device;
            deviceLabel.style.width = 50;
            EnumField deviceTypeField = new EnumField(Condition.Device);
            deviceTypeField.RegisterValueChangedCallback(evt =>
            {
                Condition.Device = (DeviceType)evt.newValue;
            });
            lower.Add(deviceLabel);
            lower.Add(deviceTypeField);
            lower.style.width = 180;
            lower.style.backgroundColor = new Color(0.14f, 0.14f, 0.14f, 0.9f);
            mainContainer.Add(lower);
            RefreshPorts();
            RefreshExpandedState();
        }
    }
}
