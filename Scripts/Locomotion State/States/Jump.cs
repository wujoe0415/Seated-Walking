using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocomotionStateMachine
{
    public class Jump : LocomotionState
    {
        private Rigidbody _rigidbody;
        [Range(1f, 100f)]
        public float Force = 10f;

        public override void StateMovement()
        {
            Debug.Log("Jump");
            _rigidbody.AddForce(Vector3.up * Force);
        }
    }
}
