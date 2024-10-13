using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace LocomotionStateMachine
{
    public interface IMovement {
        public void StateMovement();
    }
    public class LocomotionState : MonoBehaviour, IMovement
    {
        [SerializeField]
        protected string _state = "Idle";
        public string State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }

        public List<StateTransition> stateGraph = new List<StateTransition>();
        public LocomotionState ChangeState(MovementEnumerator currentState, bool isMoniter = false)
        {
            //Debug.Log((DeviceType)inputDevice + " " + (Movement)inputMovement + " " + inputTime);
            foreach (StateTransition state in stateGraph)
            {
                if (state.CanTransit(currentState, isMoniter/*inputDevice, inputMovement, inputTime*/))
                {
                    return state.NextState;
                }
            }
            return null;
        }
        
        public void AddTransition(StateTransition s)
        {
            stateGraph.Add(s);
        }
        public void ResetState()
        {
            for (int i = 0; i < stateGraph.Count; i++)
                for (int j = 0; j < stateGraph[i].Conditions.Count; j++)
                    stateGraph[i].Conditions[j].ResetCondition();
        }
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
