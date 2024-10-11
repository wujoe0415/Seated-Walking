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
            Condition.LastDeviceState = condition.LastDeviceState == HistoryState.Any?HistoryState.LastSteppedToe : condition.LastDeviceState;
            EnumField lastStateField = new EnumField("Condition", Condition.LastDeviceState);
            lastStateField.RegisterValueChangedCallback(evt =>
            {
                Condition.LastDeviceState = (HistoryState)evt.newValue;
            });
            mainContainer.Add(lastStateField);

            // Dropdown for DeviceType
            Condition.Device = condition.Device;
            EnumField deviceTypeField = new EnumField("was", Condition.Device);
            deviceTypeField.RegisterValueChangedCallback(evt =>
            {
                Condition.Device = (DeviceType)evt.newValue;
            });
            mainContainer.Add(deviceTypeField);

            RefreshPorts();
            RefreshExpandedState();
        }
    }
}
