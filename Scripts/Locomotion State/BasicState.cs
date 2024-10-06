using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LocomotionStateMachine.StateCondition;

namespace LocomotionStateMachine
{
    [System.Serializable]
    public enum StepType
    {
        LeftShoeBall,
        LeftShoeHeel,
        RightShoeBall,
        RightShoeHeel,
    }

    public class State
    {
        [HideInInspector]
        public StepType StepType = (StepType)0;
        [HideInInspector]
        public int Value = 0;
    }
    [System.Serializable]
    public class StateCondition
    {
        [System.Serializable]
        public enum Operator
        {
            Greater,
            GreaterEqual,
            Equal,
            NotEqual,
            LessEqual,
            Less,
        }

        public StepType Type = (StepType)0;
        public Operator NumericalOperator = (Operator)0;
        public int ThresholdValue = 0;
        public bool IsSatisfied(State s)
        {
            if (Type != s.StepType)
                return false;
            else
                return NumericalOperator switch
                {
                    Operator.Greater => s.Value > ThresholdValue,
                    Operator.GreaterEqual => s.Value >= ThresholdValue,
                    Operator.Equal => s.Value == ThresholdValue,
                    Operator.NotEqual => s.Value != ThresholdValue,
                    Operator.LessEqual => s.Value <= ThresholdValue,
                    Operator.Less => s.Value < ThresholdValue,
                    _ => false // Default
                };
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
            XOR
        }

        // If condition pass, would change to this state.
        public BasicState NextState = new BasicState();
        public List<StateCondition> Condition = new List<StateCondition>();
        public BooleanOperator Operator = (BooleanOperator)0;
        public bool IsSatisfied(State s)
        {
            bool flag = Condition[0].IsSatisfied(s);
            for (int i = 1; i < Condition.Count; i++)
            {
                switch (Operator)
                {
                    case BooleanOperator.AND:
                        flag &= Condition[i].IsSatisfied(s);
                        break;
                    case BooleanOperator.OR:
                        flag |= Condition[i].IsSatisfied(s);
                        break;
                    case BooleanOperator.XOR:
                        flag ^= Condition[i].IsSatisfied(s);
                        break;
                    default:
                        flag = false; break;
                }
            }
            return flag;
        }

    }

    public class BasicState : MonoBehaviour
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
        public BasicState ChangeState(State inputState)
        {
            Debug.Log((StepType)inputState.StepType + " " + inputState.Value);
            foreach (StateTransition state in stateGraph)
            {
                if (state.IsSatisfied(inputState))
                    return state.NextState;
            }
            return this;
        }

    }
}
