using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public MovementEnumerator PreviousMovement;
        public MovementEnumerator TriggerMovement;

        public HistoryState LastDeviceState = HistoryState.Any;
        public DeviceType Device;

        public StateCondition()
        {
            PreviousMovement = new MovementEnumerator();
            TriggerMovement = new MovementEnumerator();
            LastDeviceState = HistoryState.Any;
            Device = DeviceType.LeftToe;
        }
        //[Tooltip("Hold time in seconds")]
        //public float HoldTime = 0; // 0 means no hold time required, trigger immediately.
        
        public bool IsSatisfied(MovementEnumerator l, MovementEnumerator c/*, float t*/)
        {
            Debug.Log($"Previous Movement {isMatch(PreviousMovement, l)}\nTrigger Movement {isMatch(TriggerMovement, c)}\n HistoryMovemnt {isMatchHistory()}");

            return isMatch(PreviousMovement, l) & isMatch(TriggerMovement, c) & isMatchHistory();
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
        private bool isMatchHistory()
        {
            if(LastDeviceState == HistoryState.Any)
                return true;

            switch (LastDeviceState)
            {
                case HistoryState.LastSteppedToe:
                    return HistoryRecorder.s_LastToeStep == Device;
                    break;
                case HistoryState.LastSteppedHeel:
                    return HistoryRecorder.s_LastHeelStep == Device;
                    break;
                case HistoryState.LastSteppedLeftShoe:
                    return HistoryRecorder.s_LastLeftShoeStep == Device;
                    break;
                case HistoryState.LastSteppedRightShoe:
                    return HistoryRecorder.s_LastRightShoeStep == Device;
                    break;
                default:
                    return false;
            }

        }
    }
    [System.Serializable]
    public class StateTransition
    {
        [System.Serializable]
        public enum BooleanOperator
        {
            AND,
            OR,
            XOR,
        }

        // If condition pass, would change to this state.
        public LocomotionState NextState = new LocomotionState();
        public List<StateCondition> Condition = new List<StateCondition>();
        public BooleanOperator Operator = (BooleanOperator)0;
        public bool CanTransit(MovementEnumerator l, MovementEnumerator c/*, float t*/)
        {
            bool flag = Condition[0].IsSatisfied(l, c);
            for (int i = 1; i < Condition.Count; i++)
            {
                bool condiFlag = Condition[i].IsSatisfied(l, c);
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
        public LocomotionState ChangeState(MovementEnumerator lastState, MovementEnumerator currentState)
        {
            //Debug.Log((DeviceType)inputDevice + " " + (Movement)inputMovement + " " + inputTime);
            foreach (StateTransition state in stateGraph)
            {
                if (state.CanTransit(lastState, currentState/*inputDevice, inputMovement, inputTime*/))
                {
                    Debug.Log("Transit " + state.NextState.State);
                    return state.NextState;
                }
            }
            return null;
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
