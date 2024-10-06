using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace LocomotionStateMachine
{
    public class Backwarding : LocomotionState
    {
        public float MoveDuration = 0.001f;
        public float Distance = 1f;
        public override void StateMovement()
        {
            Debug.Log("Backwarding");
            Player.transform.DOMove(Player.transform.position - Player.transform.forward * Distance, MoveDuration);
        }
    }
}
