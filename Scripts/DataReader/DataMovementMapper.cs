using System;
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
    [System.Flags]
    public enum Movement
    {
        Ground = 1,
        Hang = 2,
        Step = 4,
        Raise = 8,
    }
    [System.Serializable]
    public class ShoeState
    {
        public DeviceType Device;
        public bool IsDown;
        public float Time;
        public ShoeState(DeviceType device, bool isDown, float time)
        {
            Device = device;
            IsDown = isDown;
            Time = time;
        }
    }
    [System.Serializable]
    public class MovementEnumerator{
        public Movement LeftToe;
        public Movement LeftHeel;
        public Movement RightToe;
        public Movement RightHeel;
        public MovementEnumerator()
        {
            LeftToe = Movement.Ground | Movement.Hang | Movement.Step | Movement.Raise;
            LeftHeel = Movement.Ground | Movement.Hang | Movement.Step | Movement.Raise;
            RightToe = Movement.Ground | Movement.Hang | Movement.Step | Movement.Raise;
            RightHeel = Movement.Ground | Movement.Hang | Movement.Step | Movement.Raise;
        }
        public bool isAllEverything()
        {
            return LeftToe == (Movement.Ground | Movement.Hang | Movement.Step | Movement.Raise) &&
                LeftHeel == (Movement.Ground | Movement.Hang | Movement.Step | Movement.Raise) &&
                RightToe == (Movement.Ground | Movement.Hang | Movement.Step | Movement.Raise) &&
                RightHeel == (Movement.Ground | Movement.Hang | Movement.Step | Movement.Raise);
        }
    }
    public class HistoryRecorder
    {
        public static DeviceType s_LastToeStep;
        public static DeviceType s_LastHeelStep;
        public static DeviceType s_LastLeftShoeStep;
        public static DeviceType s_LastRightShoeStep;

        public static void UpdateStep(DeviceType d)
        {
            if (d == DeviceType.LeftToe)
            {
                s_LastToeStep = d;
                s_LastLeftShoeStep = d;
            }
            else if (d == DeviceType.LeftHeel)
            {
                s_LastHeelStep = d;
                s_LastLeftShoeStep = d;
            }
            else if (d == DeviceType.RightToe)
            {
                s_LastToeStep = d;
                s_LastRightShoeStep = d;
            }
            else
            {
                s_LastHeelStep = d;
                s_LastRightShoeStep = d;
            }
        }
    }


    public class DataMovementMapper : MonoBehaviour
    {
        //[HideInInspector]
        public ShoeState[] ShoeStates = new ShoeState[4];
        public MovementEnumerator PreviousShoesStates;
        public MovementEnumerator CurrentShoesStates;
        public int RaiseThreshold = 500;
        public float StepThreshold = 5f;
        public HistoryRecorder HistoryStates;
        //public DataReader[] SerialDataReader;

        public Action<MovementEnumerator, bool> OnChangeState;

        private void Awake()
        {
            ShoeStates[0] = new ShoeState(DeviceType.LeftToe, true, 0);
            ShoeStates[1] = new ShoeState(DeviceType.LeftHeel, true, 0);
            ShoeStates[2] = new ShoeState(DeviceType.RightToe, true, 0);
            ShoeStates[3] = new ShoeState(DeviceType.RightHeel, true, 0);
        }
        private bool[] _currentChanges = new bool[4]; 
        public void OnEnable()
        {
            SerialDataReader.OnValueChange += UpdateValue;
        }
        public void OnDisable()
        {
            SerialDataReader.OnValueChange -= UpdateValue;
        }
        public void UpdateValue(DeviceType s, int value)
        {
            ShoeStates[(int)s].Time += Time.deltaTime;
            Movement previousValue = GetDeviceMovement(s);

            if (ShoeStates[(int)s].IsDown != value < RaiseThreshold)
                ShoeStates[(int)s].Time = 0;
            ShoeStates[(int)s].IsDown = value < RaiseThreshold;

            if (value > RaiseThreshold) 
            {
                if (ShoeStates[(int)s].Time > StepThreshold)
                    SetDeviceMovement(s, Movement.Hang);
                else
                    SetDeviceMovement(s, Movement.Raise);
            }
            else if (value < RaiseThreshold)
            {
                if (ShoeStates[(int)s].Time > StepThreshold)
                    SetDeviceMovement(s, Movement.Ground);
                else
                    SetDeviceMovement(s, Movement.Step);
            }
            
            // Change state
            if (previousValue != GetDeviceMovement(s))
            {
                OnChangeState?.Invoke(CurrentShoesStates, false);
                if (GetDeviceMovement(s) == Movement.Step)
                    HistoryRecorder.UpdateStep(s);
                PreviousShoesStates = CurrentShoesStates;
            }
        }
        public Movement GetDeviceMovement(DeviceType device)
        {
            if (device == DeviceType.LeftToe)
                return CurrentShoesStates.LeftToe;
            else if (device == DeviceType.LeftHeel)
                return CurrentShoesStates.LeftHeel;
            else if (device == DeviceType.RightToe)
                return CurrentShoesStates.RightToe;
            else
                return CurrentShoesStates.RightHeel;
        }
        public void SetDeviceMovement(DeviceType device, Movement movement)
        {
            if (device == DeviceType.LeftToe)
                CurrentShoesStates.LeftToe = movement;
            else if (device == DeviceType.LeftHeel)
                CurrentShoesStates.LeftHeel = movement;
            else if (device == DeviceType.RightToe)
                CurrentShoesStates.RightToe = movement;
            else
                CurrentShoesStates.RightHeel = movement;
        }

        private void Update()
        {
            OnChangeState?.Invoke(CurrentShoesStates, true);
        }
    }
}