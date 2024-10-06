using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocomotionStateMachine
{
    public class LocomotionStateMachine : MonoBehaviour
    {
        public DataMovementMapper MovementMapper;

        public List<LocomotionState> StateGraph = new List<LocomotionState>();

        public LocomotionState CurrentState;
        public List<string> StateHistory = new List<string>();

        public float IdleThreshold = 1f;
        private float _idleTimer = 0f;
        private void OnEnable()
        {
            CurrentState = StateGraph[0];
        }
        private void Update()
        {
            for (int i = 0; i < MovementMapper.CurrentShoeStates.Length; i++)
            {
                ChangeState(MovementMapper.HistoryShoeStates, MovementMapper.CurrentShoeStates[i].Device, MovementMapper.CurrentShoeStates[i].Movement, MovementMapper.CurrentShoeStates[i].Time);
            }
        }
        public void ChangeState(MovementEnumerator lastState, DeviceType inputDevice, Movement inputMovement, float inputTime)
        {
            string state = CurrentState.State; 
            CurrentState = (LocomotionState)CurrentState.ChangeState(lastState, inputDevice, inputMovement, inputTime);
            
            if (state != CurrentState.State) // Change State
            {
                StateHistory.Add(state);
                CurrentState.StateMovement(); // TODO: Check whether call when changing state
                _idleTimer = 0f;
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
