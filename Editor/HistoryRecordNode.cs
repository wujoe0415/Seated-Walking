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
            EnumField lastStateField = new EnumField("Condition", Condition.LastDeviceState);
            lastStateField.RegisterValueChangedCallback(evt =>
            {
                Condition.LastDeviceState = (HistoryState)evt.newValue;
            });
            mainContainer.Add(lastStateField);

            // Dropdown for DeviceType
            EnumField deviceTypeField = new EnumField("is", Condition.Device);
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
