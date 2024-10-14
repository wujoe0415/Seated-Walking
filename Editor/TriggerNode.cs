using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;   
using System;
using UnityEngine;

namespace LocomotionStateMachine {
    public class TriggerNode : ConditionNode
    {
        public TriggerNode(StateCondition condition, bool isEntry): base(condition, isEntry)
        {
            base.title = "Trigger Condition";

            mainContainer.Add(new Label("Trigger Movement"));
            mainContainer.Add(CreateMovementEnumeratorUI());

            RefreshPorts();
            RefreshExpandedState();
            styleSheets.Add(Resources.Load<StyleSheet>("ConditionNode"));
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
