using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocomotionStateMachine
{
    public class Forwarding : LocomotionState
    {
        public float MoveDuration = 0.001f;
        public float Distance = 3f;
        public override void StateAction()
        {
            base.StateAction();
            Debug.Log("Forwarding");
            Player.transform.position += Camera.main.transform.forward * Distance;
        }
    }
}
