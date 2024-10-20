using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace LocomotionStateMachine
{
    public class HoldStillNode : ConditionNode
    {
        public HoldStillNode(StateCondition condition, bool isEntry) : base(condition, isEntry)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("ConditionNode"));
            base.title = "Hold Still";
            VisualElement main = new VisualElement();
            main.Add(CreateMovementEnumeratorUI());
            // Create a new FloatField and set its initial value
            
            FloatField floatField = new FloatField("Hold Still Duration");
            
            if (condition.Duration == 0f)
                condition.Duration = 1f;
            floatField.value = condition.Duration; // Initialize the field with the current value of floatValue
            // Handle the change event to update the floatValue variable
            floatField.RegisterValueChangedCallback(evt =>
            {
                base.Condition.Duration = evt.newValue;
            });
            main.Add(floatField);
            main.style.backgroundColor = new Color(0.14f, 0.14f, 0.14f, 0.9f);

            // Add the FloatField to the node's main container
            mainContainer.Add(main);

            RefreshPorts();
            RefreshExpandedState();
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
            return fieldContainer;
        }
    }
}
