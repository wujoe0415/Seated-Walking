using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LocomotionStateMachine
{
    public class LocomotionParser : MonoBehaviour
    {
        public StateMachineContainer Container;
        [SerializeField] private TextMeshProUGUI locomotionText;
        [SerializeField] private Button choicePrefab;
        [SerializeField] private Transform buttonContainer;

        private void Start()
        {
            //var narrativeData = container.NodeLinks.First(); //Entrypoint node
            //ProceedToNarrative(narrativeData.TargetNodeGUID);
            ParseStateMachineContainer();
        }
        public void ParseStateMachineContainer()
        {
            if (Container == null)
            {
                Debug.LogError("StateMachineContainer reference is missing. Please assign it in the Inspector.");
                return;
            }

            // Parse NodeLinks
            foreach (var nodeLink in Container.NodeLinks)
            {
                Debug.Log($"NodeLink: From {nodeLink.BaseNodeGUID} to {nodeLink.TargetNodeGUID}");
                // Add any additional processing logic here
            }

            // Parse LocomotionNodeData
            foreach (var locomotionNode in Container.LocomotionNodeData)
            {
                Debug.Log($"LocomotionNode: {locomotionNode.LocomotionStateName}");
                // Process the locomotion node's LocomotionState data, if needed
            }

            // Parse TransitionNodeData
            foreach (var transitionNode in Container.TransitionNodeData)
            {
                Debug.Log($"TransitionNode: GUID={transitionNode.Operator}");
                // Additional processing as required
            }

            // Parse ConditionNodeData
            foreach (var conditionNode in Container.ConditionNodeData)
            {
                Debug.Log($"ConditionNode: {conditionNode.Condition}");
                // Handle the ConditionNodeData information here
            }

            // Parse ExposedProperties
            foreach (var property in Container.ExposedProperties)
            {
                Debug.Log($"ExposedProperty: {property.PropertyName} = {property.PropertyValue}");
            }

            // Parse BlockData
            foreach (var block in Container.BlockData)
            {
                Debug.Log($"BlockData: Block Name = {block}");
                // Process each BlockData here
            }
        }
        private void ProceedToNarrative(string narrativeDataGUID)
        {
            var text = Container.LocomotionNodeData.Find(x => x.NodeGUID == narrativeDataGUID).LocomotionStateName;
            var choices = Container.NodeLinks.Where(x => x.BaseNodeGUID == narrativeDataGUID);
            locomotionText.text = ProcessProperties(text);
            var buttons = buttonContainer.GetComponentsInChildren<Button>();
            for (int i = 0; i < buttons.Length; i++)
            {
                Destroy(buttons[i].gameObject);
            }

            foreach (var choice in choices)
            {
                var button = Instantiate(choicePrefab, buttonContainer);
                button.GetComponentInChildren<Text>().text = ProcessProperties(choice.BasePortName);
                button.onClick.AddListener(() => ProceedToNarrative(choice.TargetNodeGUID));
            }
        }

        private string ProcessProperties(string text)
        {
            foreach (var exposedProperty in Container.ExposedProperties)
            {
                text = text.Replace($"[{exposedProperty.PropertyName}]", exposedProperty.PropertyValue);
            }
            return text;
        }
    }
}
