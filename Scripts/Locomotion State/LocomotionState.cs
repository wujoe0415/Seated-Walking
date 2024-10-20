using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LocomotionStateMachine
{
    public interface IMovement {
        public void StateAction();
    }
    public class LocomotionState : MonoBehaviour, IMovement
    {
        protected string _state = "Idle";
        public string State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }
        [HideInInspector]
        public List<StateTransition> stateGraph = new List<StateTransition>();
        public LocomotionState ChangeState(MovementEnumerator currentState, bool isMoniter = false)
        {
            // Debug.Log(currentState.LeftToe + " " + currentState.LeftHeel + " " + currentState.RightToe + " " + currentState.RightHeel);
            for (int i = 0;i< stateGraph.Count;i++)
            {
                // Debug.Log(stateGraph[i].CanTransit(currentState, isMoniter));
                if (stateGraph[i].CanTransit(currentState, isMoniter))
                {
                    //Debug.Log($"Transition {i} achieve, go to {stateGraph[i].NextState}.");
                    return stateGraph[i].NextState;
                }
            }
            return null;
        }
        
        public void AddTransition(StateTransition s)
        {
            stateGraph.Add(s);
        }
        public void ResetState()
        {
            for (int i = 0; i < stateGraph.Count; i++)
                for (int j = 0; j < stateGraph[i].Conditions.Count; j++)
                    stateGraph[i].Conditions[j].ResetCondition();
        }
        public GameObject Player;
        [Tooltip("Once entering the state, call this event.")]
        public UnityEvent OnStateAction;

        public void OnEnable()
        {
            if (Player == null)
                Player = GameObject.FindWithTag("Player");
        }
        public void OnDisable()
        {
            stateGraph.Clear();
        }
        public virtual void StateAction()
        {
            OnStateAction.Invoke();
        }
    }
}
