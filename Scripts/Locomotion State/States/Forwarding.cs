using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocomotionStateMachine
{
    public class Forwarding : LocomotionState
    {
        public float MoveDuration = 0.001f;
        public float Distance = 0.1f;
        public override void StateAction()
        {
            base.StateAction();
            Debug.Log("Forwarding");
            Player.transform.position = Player.transform.position + Player.transform.forward * Distance;
        }
    }
}
