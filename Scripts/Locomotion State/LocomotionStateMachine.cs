using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocomotionStateMachine
{
    public class LocomotionStateMachine : MonoBehaviour
    {
        public List<LocomotionState> StateGraph = new List<LocomotionState>();

        public LocomotionState CurrentState;
        public List<string> StateHistory = new List<string>();

        public void ChangeState(State currentStep)
        {
            string state = CurrentState.State; 
            CurrentState = (LocomotionState)CurrentState.ChangeState(currentStep);
            
            if (state != CurrentState.State) // Change State
            {
                StateHistory.Add(state);
                CurrentState.StateMovement(); // TODO: Check whether call when changing state
            }
        }
    }
}
