using LocomotionStateMachine;
using System.Collections.Generic;
using UnityEngine;

namespace LocomotionStateMachine
{
    [System.Serializable]
    public class StateTransition
    {
        public StateTransition(LocomotionState state, BooleanOperator oper, List<StateCondition> conditions)
        {
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
                        flag = false;
                        break;
                }
            }
            return flag;
        }
    }
}