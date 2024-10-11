using System;
using UnityEngine;

namespace LocomotionStateMachine
{
    [Serializable]
    public class LocomotionNodeData: BasicNodeData
    { 
        public string LocomotionStateName;
        public UnityEngine.Object LocomotionState = new UnityEngine.Object() as LocomotionState;
    }
}
