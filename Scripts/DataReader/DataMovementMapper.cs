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
        public static DeviceType s_LastStepDevice;
        public static DeviceType s_LastSteppedToe;
        public static DeviceType s_LastSteppedHeel;
        public static DeviceType s_LastSteppedLeftShoe;
        public static DeviceType s_LastSteppedRightShoe;

        public static DeviceType s_LastRaisedDevice;
        public static DeviceType s_LastRaisedToe;
        public static DeviceType s_LastRaisedHeel;
        public static DeviceType s_LastRaisedLeftShoe;
        public static DeviceType s_LastRaisedRightShoe;

        public static void UpdateStep(DeviceType d)
        {
            s_LastStepDevice = d;
            if (d == DeviceType.LeftToe)
            {
                s_LastSteppedToe = d;
                s_LastSteppedLeftShoe = d;
            }
            else if (d == DeviceType.LeftHeel)
            {
                s_LastSteppedHeel = d;
                s_LastSteppedLeftShoe = d;
            }
            else if (d == DeviceType.RightToe)
            {
                s_LastSteppedToe = d;
                s_LastSteppedRightShoe = d;
            }
            else
            {
                s_LastSteppedHeel = d;
                s_LastSteppedRightShoe = d;
            }
        }
        public static void UpdateRaise(DeviceType d)
        {
            s_LastRaisedDevice = d;
            if (d == DeviceType.LeftToe)
            {
                s_LastRaisedToe = d;
                s_LastRaisedLeftShoe = d;
            }
            else if (d == DeviceType.LeftHeel)
            {
                s_LastRaisedHeel = d;
                s_LastRaisedLeftShoe = d;
            }
            else if (d == DeviceType.RightToe)
            {
                s_LastRaisedToe = d;
                s_LastRaisedRightShoe = d;
            }
            else
            {
                s_LastRaisedHeel = d;
                s_LastRaisedRightShoe = d;
            }
        }
    }


    public class DataMovementMapper : MonoBehaviour
    {
        //[HideInInspector]
        public ShoeState[] ShoeStates = new ShoeState[4];
        [HideInInspector]
        public MovementEnumerator PreviousShoesStates;
        public MovementEnumerator CurrentShoesStates;
        public int RaiseThreshold = 500;
        public float StepThreshold = 5f;
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
            KeyboardDataReader.OnValueChange += UpdateValue;
        }
        public void OnDisable()
        {
            SerialDataReader.OnValueChange -= UpdateValue;
            KeyboardDataReader.OnValueChange -= UpdateValue;
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
            else
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
                else if (GetDeviceMovement(s) == Movement.Raise)
                    HistoryRecorder.UpdateRaise(s);
                PreviousShoesStates = CurrentShoesStates;
            }
            else
                OnChangeState?.Invoke(CurrentShoesStates, true);
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
    }
}