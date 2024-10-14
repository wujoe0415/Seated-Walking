using System;

namespace LocomotionStateMachine
{
    [Serializable]
    public class TransitionNodeData : BasicNodeData
    {
        public StateTransition.BooleanOperator Operator;
    }
}
