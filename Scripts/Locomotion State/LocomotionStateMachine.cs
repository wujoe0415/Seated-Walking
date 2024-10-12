using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocomotionStateMachine
{
    public class LocomotionStateMachine : MonoBehaviour
    {
        private DataMovementMapper MovementMapper;

        private List<LocomotionState> StateGraph = new List<LocomotionState>();

        [HideInInspector]
        public LocomotionState RootState;

        [HideInInspector]
        public LocomotionState CurrentState;
        [HideInInspector]
        public List<string> StateHistory = new List<string>();

        public float IdleThreshold = 1f;
        private float _idleTimer = 0f;
        private void OnEnable()
        {
            MovementMapper = FindObjectOfType<DataMovementMapper>();
            MovementMapper.OnChangeState += ChangeState;
        }
        private void OnDisable()
        {
            MovementMapper.OnChangeState -= ChangeState;
            StateHistory.Clear();
        }
        public void AddState(LocomotionState state)
        {
            StateGraph.Add(state);
        }
        public LocomotionState GetState(string name)
        {
            return StateGraph.Find(x => x.State == name);
        }
        public void ChangeState(MovementEnumerator lastState, MovementEnumerator currentState)
        {
            if(CurrentState == null)
            {
                Debug.LogWarning("Current State is null!");
                return;
            }    
            string state = CurrentState.State;
            LocomotionState nextState = CurrentState.ChangeState(currentState);
            if (nextState != null) {
                CurrentState = nextState;
                StateHistory.Add(state);
                _idleTimer = 0f;
                CurrentState.StateMovement(); // TODO: Check whether call when changing state
            }
            else
            {
                _idleTimer += Time.deltaTime;
                if (_idleTimer > IdleThreshold)
                {
                    CurrentState = StateGraph[0];
                    _idleTimer = 0f;
                }
            }
        }
    }
}
