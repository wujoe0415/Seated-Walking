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

            mainContainer.Add(CreateMovementEnumeratorUI());

            RefreshPorts();
            RefreshExpandedState();
            styleSheets.Add(Resources.Load<StyleSheet>("ConditionNode"));
        }
        private VisualElement CreateMovementEnumeratorUI()
        {
            // Container for MovementEnumerator
            VisualElement container = new VisualElement();

            // Add movement fields
            container.Add(CreateMovementField("Left Toe", Condition.TriggerMovement.LeftToe, newValue => Condition.TriggerMovement.LeftToe = newValue,
                                              "Right Toe", Condition.TriggerMovement.RightToe, newValue => Condition.TriggerMovement.RightToe = newValue));
            container.Add(CreateMovementField("Left Heel", Condition.TriggerMovement.LeftHeel, newValue => Condition.TriggerMovement.LeftHeel = newValue,
                                              "Right Heel", Condition.TriggerMovement.RightHeel, newValue => Condition.TriggerMovement.RightHeel = newValue));

            return container;
        }
        private VisualElement CreateMovementField(string label1, Movement currentValue1, System.Action<Movement> onValueChanged1,
                                                  string label2, Movement currentValue2, System.Action<Movement> onValueChanged2)
        {
            VisualElement fieldContainer = new VisualElement();
            fieldContainer.style.flexDirection = FlexDirection.Row;
            fieldContainer.style.width = 160;
            Label fieldLabel = new Label(label1);
            EnumFlagsField movementField1 = new EnumFlagsField(currentValue1);
            movementField1.RegisterValueChangedCallback(evt =>
            {
                onValueChanged1((Movement)evt.newValue);
            });
            Label fieldLabel2 = new Label(label2);
            EnumFlagsField movementField2 = new EnumFlagsField(currentValue2);
            movementField2.RegisterValueChangedCallback(evt =>
            {
                onValueChanged2((Movement)evt.newValue);
            });
            VisualElement left = new VisualElement();
            left.style.width = 80;
            left.Add(fieldLabel);
            left.Add(movementField1);
            fieldContainer.Add(left);
            VisualElement right = new VisualElement();
            right.style.width = 80;
            right.Add(fieldLabel2);
            right.Add(movementField2);
            fieldContainer.Add(right);
            fieldContainer.style.backgroundColor = new Color(0.14f, 0.14f, 0.14f, 0.9f);

            return fieldContainer;
        }
    }
}
