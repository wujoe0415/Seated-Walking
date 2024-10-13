using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocomotionStateMachine
{
    public class LocomotionStateMachine : MonoBehaviour
    {
        private DataMovementMapper _movementMapper;
        private List<LocomotionState> _stateGraph = new List<LocomotionState>();

        [HideInInspector]
        public LocomotionState RootState;
        
        public LocomotionState CurrentState;
        [HideInInspector]
        public List<string> StateHistory = new List<string>();

        //public float IdleThreshold = 1f;
        //private float _idleTimer = 0f;
        private void OnEnable()
        {
            _movementMapper = FindObjectOfType<DataMovementMapper>();
            _movementMapper.OnChangeState += ChangeState;
        }
        private void OnDisable()
        {
            _movementMapper.OnChangeState -= ChangeState;
            StateHistory.Clear();
        }
        public void AddState(LocomotionState state)
        {
            _stateGraph.Add(state);
        }
        public LocomotionState GetState(string name)
        {
            foreach(LocomotionState state in _stateGraph)
            {
                if (state.State == name)
                    return state;
            }
            return _stateGraph.Find(x => x.State == name);
        }
        public void ChangeState(MovementEnumerator currentState, bool isMoniter)
        {
            if(CurrentState == null)
            {
                Debug.LogWarning("Current State is null!");
                return;
            }    
            string state = CurrentState.State;
            LocomotionState nextState = CurrentState.ChangeState(currentState, isMoniter);
            if (nextState != null)
            {
                CurrentState.ResetState();
                CurrentState = nextState;
                //StateHistory.Add(state);
                //_idleTimer = 0f;
                CurrentState.ResetState();
                CurrentState.StateMovement(); // TODO: Check whether call when changing state
            }
            //else
            //{
            //    _idleTimer += Time.deltaTime;
            //    if (_idleTimer > IdleThreshold)
            //    {
            //        CurrentState = _stateGraph[0];
            //        _idleTimer = 0f;
            //    }
            //}
        }
    }
}
