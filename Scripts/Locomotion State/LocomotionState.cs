using UnityEngine;

namespace LocomotionStateMachine
{
    public interface IMovement {
        public void StateMovement();
    }

    public class LocomotionState : BasicState
    {
        [HideInInspector]
        public GameObject Player;
        public void OnEnable()
        {
            if (Player == null)
                Player = GameObject.FindObjectOfType<OVRCameraRig>().gameObject;
        }
        public virtual void StateMovement()
        {
            Debug.Log("Basic Locomotion Movement");
        }
    }
}
