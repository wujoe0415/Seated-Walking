using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace LocomotionStateMachine
{
    [System.Serializable]
    public enum HistoryState {
        Any,                    // Not care
        LastSteppedToe,
        LastSteppedHeel,
        LastSteppedLeftShoe,
        LastSteppedRightShoe,
    }

    [System.Serializable]
    public class StateCondition
    {
        public MovementEnumerator TriggerMovement;
        public HistoryState LastDeviceState = HistoryState.Any;
        public DeviceType Device;
        public float Duration = 0f;
        public float _currentDuration = 0f;

        public StateCondition()
        {
            TriggerMovement = new MovementEnumerator();
            LastDeviceState = HistoryState.Any;
            Device = DeviceType.LeftToe;
            _currentDuration = 0f;
        }
        public void ResetCondition()
        {
           _currentDuration = 0f;
        }
        //[Tooltip("Hold time in seconds")]
        //public float HoldTime = 0; // 0 means no hold time required, trigger immediately.
        
        public bool IsSatisfied(MovementEnumerator c, bool isMoniter/*, float t*/)
        {
            if (!isMoniter)
                return isMatch(TriggerMovement, c) & isMatchHistory();
            else
                return checkDuration(c);
            //if (HasSatisfied)
            //    return HasSatisfied;
            //else{
            //    bool HasSatisfied = 
            //    return HasSatisfied;
            //}
        }
        private bool isMatch(MovementEnumerator m1, MovementEnumerator m2)
        {
            bool[] flags = new bool[4];
            for (int i = 0; i < flags.Length; i++)
                flags[i] = false;

            // LeftToe
            if (m1.LeftToe.HasFlag(m2.LeftToe))
                flags[0] = true;
            if (m1.LeftHeel.HasFlag(m2.LeftHeel))
                flags[1] = true;
            if (m1.RightToe.HasFlag(m2.RightToe))
                flags[2] = true;
            if (m1.RightHeel.HasFlag(m2.RightHeel))
                flags[3] = true;
            return flags[0] & flags[1] & flags[2] & flags[3];
        }
        private bool isMatch(MovementEnumerator e, DeviceType d, Movement m)
        {
            if (d == DeviceType.LeftToe)
                return e.LeftToe.HasFlag(m);
            else if (d == DeviceType.RightToe)
                return e.RightToe.HasFlag(m);
            else if (d == DeviceType.LeftHeel)
                return e.LeftHeel.HasFlag(m);
            else
                return e.RightHeel.HasFlag(m);
        } 
        private bool checkDuration(MovementEnumerator e)
        {
            if (Duration == 0f)
                return false;
            bool[] flags = new bool[4];
            for (int i = 0; i < flags.Length; i++)
                flags[i] = false;

            if (TriggerMovement.LeftToe.HasFlag(TriggerMovement.LeftToe))
                flags[0] = true;
            if (TriggerMovement.LeftHeel.HasFlag(TriggerMovement.LeftHeel))
                flags[1] = true;
            if (TriggerMovement.RightToe.HasFlag(TriggerMovement.RightToe))
                flags[2] = true;
            if (TriggerMovement.RightHeel.HasFlag(TriggerMovement.RightHeel))
                flags[3] = true;
            if (flags[0] & flags[1] & flags[2] & flags[3])
            {
                if (_currentDuration > Duration)
                    return true;
                else
                    _currentDuration += Time.deltaTime;
            }
            else
                _currentDuration = 0f;
            return false;
        }
        private bool isMatchHistory()
        {
            if(LastDeviceState == HistoryState.Any)
                return true;

            switch (LastDeviceState)
            {
                case HistoryState.LastSteppedToe:
                    return HistoryRecorder.s_LastToeStep == Device;
                case HistoryState.LastSteppedHeel:
                    return HistoryRecorder.s_LastHeelStep == Device;
                case HistoryState.LastSteppedLeftShoe:
                    return HistoryRecorder.s_LastLeftShoeStep == Device;
                case HistoryState.LastSteppedRightShoe:
                    return HistoryRecorder.s_LastRightShoeStep == Device;
                default:
                    return false;
            }

        }
    }
    [System.Serializable]
    public class StateTransition
    {
        public StateTransition(LocomotionState state, BooleanOperator oper, List<StateCondition> conditions) {
            NextState = state;
            Operator = oper;
            Conditions = conditions;
        }
        [System.Serializable]
        public enum BooleanOperator
        {
            AND,
            OR,
            XOR,
        }

        // If condition pass, would change to this state.
        public LocomotionState NextState = new LocomotionState();
        public List<StateCondition> Conditions = new List<StateCondition>();
        public BooleanOperator Operator = (BooleanOperator)0;
        public bool CanTransit(MovementEnumerator c, bool isMoniter/*, float t*/)
        {
            bool flag = Conditions[0].IsSatisfied(c, isMoniter);
            for (int i = 1; i < Conditions.Count; i++)
            {
                bool condiFlag = Conditions[i].IsSatisfied(c, isMoniter);
                switch (Operator)
                {
                    case BooleanOperator.AND:
                        flag &= condiFlag;
                        break;
                    case BooleanOperator.OR:
                        flag |= condiFlag;
                        break;
                    case BooleanOperator.XOR:
                        flag ^= condiFlag;
                        break;
                    default:
                        flag = false; break;
                }
            }
            return flag;
        }
    }
    public interface IMovement {
        public void StateMovement();
    }
    public class LocomotionState : MonoBehaviour, IMovement
    {
        [SerializeField]
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

        public List<StateTransition> stateGraph = new List<StateTransition>();
        public LocomotionState ChangeState(MovementEnumerator currentState, bool isMoniter = false)
        {
            //Debug.Log((DeviceType)inputDevice + " " + (Movement)inputMovement + " " + inputTime);
            foreach (StateTransition state in stateGraph)
            {
                if (state.CanTransit(currentState, isMoniter/*inputDevice, inputMovement, inputTime*/))
                {
                    return state.NextState;
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
        [HideInInspector]
        public GameObject Player;
        public void OnEnable()
        {
            if (Player == null)
                Player = GameObject.FindObjectOfType<OVRCameraRig>().gameObject;
        }
        public virtual void StateMovement()
        {
            Debug.Log("Basic Locomotion Movement");
        }
    }
}
