using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocomotionStateMachine {
    [System.Serializable]
    public enum DeviceType
    {
        LeftToe = 0,
        LeftHeel = 1,
        RightToe = 2,
        RightHeel = 3,
    }
    [System.Serializable]
    public enum Movement
    {
        Raise = 0,
        Lower = 1,
    }
    [System.Serializable]
    public class ShoeState
    {
        public DeviceType Device;
        public Movement Movement;
        public float Time;
        public ShoeState(DeviceType device, Movement movement, float time)
        {
            Device = device;
            Movement = movement;
            Time = time;
        }
    }
    public class DataMovementMapper : MonoBehaviour
    {
        [HideInInspector]
        public ShoeState[] CurrentShoeStates = new ShoeState[4];
        public MovementEnumerator HistoryShoeStates;
        public int RaiseThreshold = 500;
        private float _stepThreshold = 4.5f;
        //public DataReader[] SerialDataReader;

        private void Awake()
        {
            CurrentShoeStates[0] = new ShoeState(DeviceType.LeftToe, Movement.Lower, 0);
            CurrentShoeStates[1] = new ShoeState(DeviceType.LeftHeel, Movement.Lower, 0);
            CurrentShoeStates[2] = new ShoeState(DeviceType.RightToe, Movement.Lower, 0);
            CurrentShoeStates[3] = new ShoeState(DeviceType.RightHeel, Movement.Lower, 0);
        }
        private bool[] _currentChanges = new bool[4]; 
        public void OnEnable()
        {
            SerialDataReader.OnValueChange += UpdateValue;
            StartCoroutine(StepMonitor());
        }
        public void OnDisable()
        {
            SerialDataReader.OnValueChange -= UpdateValue;
            StopAllCoroutines();
        }
        public void UpdateValue(DeviceType s, int value)
        {
            HistoryShoeStates = HistoryShoeStates | EncodingState(CurrentShoeStates[0], CurrentShoeStates[1], CurrentShoeStates[2], CurrentShoeStates[3]);

            CurrentShoeStates[(int)s].Time += Time.deltaTime;
            Movement previousValue = CurrentShoeStates[(int)s].Movement;
            CurrentShoeStates[(int)s].Movement = value > RaiseThreshold ? Movement.Raise : Movement.Lower;
            if (previousValue != CurrentShoeStates[(int)s].Movement)
                CurrentShoeStates[(int)s].Time = 0;
            
        }

        private MovementEnumerator EncodingState(ShoeState lt, ShoeState lh, ShoeState rt, ShoeState rh)
        {
            return (MovementEnumerator)(1 << (2 << (int)lt.Movement) >> (int)lt.Movement) |
                   (MovementEnumerator)(1 << (2 << (int)lh.Movement) >> (int)lh.Movement) |
                   (MovementEnumerator)(1 << (2 << (int)rt.Movement) >> (int)rt.Movement) |
                   (MovementEnumerator)(1 << (2 << (int)rh.Movement) >> (int)rh.Movement);
        }

        private IEnumerator StepMonitor()
        {
            while (true)
            {
                if (CurrentShoeStates[0].Time > _stepThreshold)
                    HistoryShoeStates &= ~(MovementEnumerator)512; // no step
                else if (CurrentShoeStates[0].Movement == Movement.Lower && CurrentShoeStates[0].Time < _stepThreshold)
                    HistoryShoeStates |= (MovementEnumerator)512;  // step

                if (CurrentShoeStates[1].Time > _stepThreshold)
                    HistoryShoeStates &= ~(MovementEnumerator)1024;
                else if (CurrentShoeStates[1].Movement == Movement.Lower && CurrentShoeStates[1].Time < _stepThreshold)
                    HistoryShoeStates |= (MovementEnumerator)1024;

                if (CurrentShoeStates[2].Time > _stepThreshold)
                    HistoryShoeStates &= ~(MovementEnumerator)2048;
                else if (CurrentShoeStates[2].Movement == Movement.Lower && CurrentShoeStates[2].Time < _stepThreshold)
                    HistoryShoeStates |= (MovementEnumerator)2048;
                
                if (CurrentShoeStates[3].Time > _stepThreshold)
                    HistoryShoeStates &= ~(MovementEnumerator)4096;
                else if (CurrentShoeStates[3].Movement == Movement.Lower && CurrentShoeStates[3].Time < _stepThreshold)
                    HistoryShoeStates |= (MovementEnumerator)4096;
                    
                yield return null;
            }
        }
    }
}