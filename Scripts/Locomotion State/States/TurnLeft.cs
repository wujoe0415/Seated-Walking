using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace LocomotionStateMachine
{
    public class TurnLeft : LocomotionState
    {
        [Range(0f, 90f)]
        public float RotateAngle = 45f;
        public float RotateDuration = 0.001f; // duration for a dotween transform 

        public override void StateMovement()
        {
            Debug.Log("Turn Left");
            Vector3 rotateAngle = Vector3.up * RotateAngle;
            Player.transform.DORotate(Player.transform.eulerAngles + rotateAngle, RotateDuration);
        }
    }
}
