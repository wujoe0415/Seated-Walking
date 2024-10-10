using System;
using UnityEngine;

namespace LocomotionStateMachine
{
    [Serializable]
    public class ConditionNodeData: BasicNodeData
    {
        public StateCondition Condition;
    }
}