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
            mainContainer.Add(new Label("Hold Still Movement"));
            mainContainer.Add(CreateMovementEnumeratorUI());
            // Create a new FloatField and set its initial value
            
            FloatField floatField = new FloatField("Hold Still Duration");
            
            if (condition.Duration == 0f)
                condition.Duration = 1.5f;
            floatField.value = condition.Duration; // Initialize the field with the current value of floatValue
            // Handle the change event to update the floatValue variable
            floatField.RegisterValueChangedCallback(evt =>
            {
                base.Condition.Duration = evt.newValue;
            });

            // Add the FloatField to the node's main container
            mainContainer.Add(floatField);

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

            styleSheets.Add(Resources.Load<StyleSheet>("ConditionNode"));
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
