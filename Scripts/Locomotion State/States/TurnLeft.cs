using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocomotionStateMachine
{
    public class TurnLeft : LocomotionState
    {
        [Range(0f, 90f)]
        public float RotateAngle = 45f;
        public float RotateDuration = 0.001f; // duration for a dotween transform 

        public override void StateAction()
        {
            base.StateAction();
            Vector3 rotateAngle = Vector3.up * RotateAngle;
            Player.transform.RotateAround(Player.transform.position, Vector3.down, RotateAngle);
        }
    }
}
