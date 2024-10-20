using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace LocomotionStateMachine
{
    [RequireComponent(typeof(LocomotionStateMachine))]
    public class LocomotionParser : MonoBehaviour
    {
        [System.Serializable]
        public class StateMap
        {
            public string key;
            public LocomotionState value;
        }

        public StateMachineContainer Container;
        private LocomotionStateMachine _stateMachine;
        public List<StateMap> StateMaps = new List<StateMap>();

        private void Start()
        {
            _stateMachine = GetComponent<LocomotionStateMachine>();
            if (_stateMachine == null)
                _stateMachine = gameObject.AddComponent<LocomotionStateMachine>();
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
            // Construct Graph
            foreach (var locomotionNode in Container.LocomotionNodeData)
            {
                LocomotionState s = StateMaps.Find(x => x.key == locomotionNode.LocomotionStateName).value as LocomotionState;
                s.State = locomotionNode.LocomotionStateName;
                _stateMachine.AddState(s);
            }
            // Set up Root State
            foreach (var locomotionNode in Container.NodeLinks)
            {
                if (locomotionNode.BasePortName == "Root")
                {
                    string rootName = Container.LocomotionNodeData.Find(x => x.NodeGUID == locomotionNode.TargetNodeGUID).LocomotionStateName;
                    _stateMachine.RootState = StateMaps.Find(x=>x.key == rootName).value as LocomotionState;
                    _stateMachine.CurrentState = _stateMachine.RootState;
                    break;
                }
            }

            // Set up Each State
            foreach (var locomotionNode in Container.LocomotionNodeData)
            {
                var state = _stateMachine.GetState(locomotionNode.LocomotionStateName);
                state.State = locomotionNode.LocomotionStateName;
                var outputLinks = Container.NodeLinks.Where(x => x.BaseNodeGUID == locomotionNode.NodeGUID);
                if (outputLinks.Count() == 0)
                    continue;
                state.stateGraph = new List<StateTransition>();
                foreach (var nodeLink in outputLinks)
                {
                    // find transition node
                    var transitionNode = Container.TransitionNodeData.Find(x => x.NodeGUID == nodeLink.TargetNodeGUID);
                    
                    var transitionNodeuid = Container.NodeLinks.Find(x => x.BaseNodeGUID == transitionNode.NodeGUID).TargetNodeGUID;
                    string targetStateName = Container.LocomotionNodeData.Find(x => x.NodeGUID == transitionNodeuid).LocomotionStateName;
                    LocomotionState destination = _stateMachine.GetState(targetStateName);
                    var conditions = Container.NodeLinks.Where(x => x.TargetNodeGUID == transitionNode.NodeGUID);
                    var conditionNodes = Container.ConditionNodeData.Where(x => conditions.Any(y => y.BaseNodeGUID == x.NodeGUID));
                    List<StateCondition> stateConditions = new List<StateCondition>();
                    foreach (var conditionNode in conditionNodes)
                        stateConditions.Add(conditionNode.Condition);

                    StateTransition transition = new StateTransition(destination, transitionNode.Operator, stateConditions);
                    state.AddTransition(transition);                    
                }
            }
        }
        private void OnValidate()
        {
            // Initialize the container if it hasn¡¦t been set
            if (Container == null)
            {
                StateMaps.Clear();
                return;
            }
            // Create a HashSet of keys from the List for quick lookup
            HashSet<string> existingKeys = new HashSet<string>();
            foreach (var locomotionKey in StateMaps)
                existingKeys.Add(locomotionKey.key);
            foreach (var locomotionNode in Container.LocomotionNodeData)
            {
                if (existingKeys.Contains(locomotionNode.LocomotionStateName))
                    continue;
                existingKeys.Add(locomotionNode.LocomotionStateName);
                StateMaps.Add(new StateMap
                {
                    key = locomotionNode.LocomotionStateName,
                    value = null
                });
            }
        }
    }
}
