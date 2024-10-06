using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocomotionStateMachine {
    public class DataManager : MonoBehaviour
    {
        public void OnEnable()
        {
            SerialDataReader.OnValueChange += UpdateValue;
        }
        public void OnDisable()
        {
            SerialDataReader.OnValueChange -= UpdateValue;
        }
        public void UpdateValue(StepType s, int value)
        {

        }
    }
}