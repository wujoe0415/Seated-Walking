using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LocomotionStateMachine.StateCondition;

namespace LocomotionStateMachine
{
    [System.Flags]
        public enum MovementEnumerator{
            LeftToeRaise = 2,
            LeftToeLower = 4,
            LeftHeelRaise = 8, 
            LeftHeelLower = 16,
            RightToeRaise = 32,
            RightToeLower = 64,
            RightHeelRaise = 128,
            RightHeelLower = 256,
            
            LeftToeStep = 512,
            LeftHeelStep = 1024,
            RightToeStep = 2048,
            RightHeelStep = 4096,
        }
    [System.Serializable]
    public class StateCondition
    {
        public MovementEnumerator PreviousMovement = (MovementEnumerator)1;
        public MovementEnumerator TriggerMovement = (MovementEnumerator)1;
        
        [Tooltip("Hold time in seconds")]
        public float HoldTime = 0; // 0 means no hold time required, trigger immediately.
        private bool HasSatisfied = false;
        
        public bool IsSatisfied(MovementEnumerator l, DeviceType d, Movement m, float t)
        {
            if (HasSatisfied)
                return HasSatisfied;
            else{
                bool HasSatisfied = IsMatchPrevious(l) && IsMatchTrigger(d, m) && (t > HoldTime);
                return HasSatisfied;
            }
        }
        private bool IsMatchTrigger(DeviceType d, Movement m)
        {
            // Encode
            MovementEnumerator input = (MovementEnumerator)(1 << (2 << (int)d) >> (int)m);
            return (TriggerMovement ^ (TriggerMovement | input)) > 0;
        } 
        private bool IsMatchPrevious(MovementEnumerator lastState)
        {
            // if both previous state and lastState contain zero at the same position, return false
            return ((~PreviousMovement & ~lastState) == 0);
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
        public bool CanTransit(MovementEnumerator l, DeviceType d, Movement m, float t)
        {
            bool flag = Condition[0].IsSatisfied(l, d, m, t);
            for (int i = 1; i < Condition.Count; i++)
            {
                switch (Operator)
                {
                    case BooleanOperator.AND:
                        flag &= Condition[i].IsSatisfied(l, d, m, t);
                        break;
                    case BooleanOperator.OR:
                        flag |= Condition[i].IsSatisfied(l, d, m, t);
                        break;
                    case BooleanOperator.XOR:
                        flag ^= Condition[i].IsSatisfied(l, d, m, t);
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
        }

        public List<StateTransition> stateGraph = new List<StateTransition>();
        public LocomotionState ChangeState(MovementEnumerator lastState, DeviceType inputDevice, Movement inputMovement, float inputTime)
        {
            //Debug.Log((DeviceType)inputDevice + " " + (Movement)inputMovement + " " + inputTime);
            foreach (StateTransition state in stateGraph)
            {
                if (state.CanTransit(lastState, inputDevice, inputMovement, inputTime))
                    return state.NextState;
            }
            return this;
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
