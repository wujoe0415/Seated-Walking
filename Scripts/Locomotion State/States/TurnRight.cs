using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace LocomotionStateMachine
{
    public class TurnRight : LocomotionState
    {
        [Range(0f, 90f)]
        public float RotateAngle = 45f;
        public float RotateDuration = 0.001f; // duration for a dotween transform 

        public override void StateMovement()
        {
            Debug.Log("Turn Right");
            Vector3 rotateAngle = Vector3.down * RotateAngle;
            Player.transform.DORotate(Player.transform.eulerAngles + rotateAngle, RotateDuration);
        }
    }
}
