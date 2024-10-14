using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LocomotionStateMachine
{
    public class Idle : LocomotionState
    {
        public override void StateAction()
        {
            base.StateAction();
            //Debug.Log("Idle");
        }
    }
}
