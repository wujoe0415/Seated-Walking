using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocomotionStateMachine
{
    public class LocomotionStateMachine : MonoBehaviour
    {
        public DataMovementMapper MovementMapper;

        public List<LocomotionState> StateGraph = new List<LocomotionState>();

        public LocomotionState RootState;
        public LocomotionState CurrentState;
        public List<string> StateHistory = new List<string>();

        public float IdleThreshold = 1f;
        private float _idleTimer = 0f;
        private void OnEnable()
        {
            if (RootState == null)
                RootState = StateGraph[0];
            CurrentState = RootState;
            MovementMapper.OnChangeState += ChangeState;
        }
        private void OnDisable()
        {
            MovementMapper.OnChangeState -= ChangeState;
        }

        public void ChangeState(MovementEnumerator lastState, MovementEnumerator currentState)
        {
            string state = CurrentState.State;
            LocomotionState nextState = CurrentState.ChangeState(lastState, currentState);
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
