using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocomotionStateMachine
{
    public class LeftSideStep : LocomotionState
    {
        public float Distance = 3f;
        public override void StateAction()
        {
            Player.transform.position -= Camera.main.transform.right * Distance;
        }
    }
}
