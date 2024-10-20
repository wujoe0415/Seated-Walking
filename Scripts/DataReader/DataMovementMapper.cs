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
        public int Value;
        public ShoeState(DeviceType device, bool isDown, float time)
        {
            Device = device;
            IsDown = isDown;
            Time = time;
            Value = 0;
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
        public void Reset()
        {
            LeftToe = 0;
            RightHeel = 0;
            LeftHeel = 0;
            RightToe = 0;
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
            //Debug.Log("Update Last Stepped Device " + d);
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
        public static void UpdateMovement(MovementEnumerator m)
        {
            if (m.LeftToe == Movement.Step)
                UpdateStep(DeviceType.LeftToe);
            else if (m.LeftHeel == Movement.Step)
                UpdateStep(DeviceType.LeftHeel);
            else if (m.RightToe == Movement.Step)
                UpdateStep(DeviceType.RightToe);
            else if (m.RightHeel == Movement.Step)
                UpdateStep(DeviceType.RightHeel);

            if (m.LeftToe == Movement.Raise)
                UpdateRaise(DeviceType.LeftToe);
            else if (m.LeftHeel == Movement.Raise)
                UpdateRaise(DeviceType.LeftHeel);
            else if (m.RightToe == Movement.Raise)
                UpdateRaise(DeviceType.RightToe);
            else if (m.RightHeel == Movement.Raise)
                UpdateRaise(DeviceType.RightHeel);
        }
    }


    public class DataMovementMapper : MonoBehaviour
    {
        //[HideInInspector]
        public ShoeState[] ShoeStates = new ShoeState[4];
        //[HideInInspector]
        [SerializeField]
        private MovementEnumerator PreviousShoesStates = new MovementEnumerator();
        public MovementEnumerator CurrentShoesStates = new MovementEnumerator();
        public int RaiseThreshold = 100;
        public int StepThreshold = 80;
        //public DataReader[] SerialDataReader;

        public Action<MovementEnumerator, bool> OnChangeState;

        private void Awake()
        {
            ShoeStates[0] = new ShoeState(DeviceType.LeftToe, true, 0);
            ShoeStates[1] = new ShoeState(DeviceType.LeftHeel, true, 0);
            ShoeStates[2] = new ShoeState(DeviceType.RightToe, true, 0);
            ShoeStates[3] = new ShoeState(DeviceType.RightHeel, true, 0);
            CurrentShoesStates = new MovementEnumerator();
            PreviousShoesStates = new MovementEnumerator();
            PreviousShoesStates.Reset();
            CurrentShoesStates.Reset();
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
        private bool[] _hasUpdated = new bool[4] { false, false, false, false };
        public void UpdateValue(DeviceType s, int value)
        {
            ShoeStates[(int)s].Time += Time.deltaTime;
            ShoeStates[(int)s].Value = value;

            bool curDown = ShoeStates[(int)s].IsDown ? value < RaiseThreshold : value < StepThreshold;
            if (ShoeStates[(int)s].IsDown != curDown)
                ShoeStates[(int)s].Time = 0;
            ShoeStates[(int)s].IsDown = curDown;

            if (value > RaiseThreshold) 
            {
                if (ShoeStates[(int)s].Time > 0)
                    SetDeviceMovement(s, Movement.Hang);
                else
                    SetDeviceMovement(s, Movement.Raise);
            }
            else
            {
                if (ShoeStates[(int)s].Time > 0)
                    SetDeviceMovement(s, Movement.Ground);
                else
                    SetDeviceMovement(s, Movement.Step);
            }
            _hasUpdated[(int)s] = true;
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
        public void UpdateMovement() 
        {
            // Change state
            if (PreviousShoesStates.LeftToe != CurrentShoesStates.LeftToe ||
                PreviousShoesStates.LeftHeel != CurrentShoesStates.LeftHeel ||
                PreviousShoesStates.RightToe != CurrentShoesStates.RightToe ||
                PreviousShoesStates.RightHeel != CurrentShoesStates.RightHeel)
            {
                OnChangeState?.Invoke(CurrentShoesStates, false);
                PreviousShoesStates.LeftToe = CurrentShoesStates.LeftToe;
                PreviousShoesStates.LeftHeel = CurrentShoesStates.LeftHeel;
                PreviousShoesStates.RightToe = CurrentShoesStates.RightToe;
                PreviousShoesStates.RightHeel = CurrentShoesStates.RightHeel;

                HistoryRecorder.UpdateMovement(PreviousShoesStates);
            }
            else
                OnChangeState?.Invoke(CurrentShoesStates, true);
        }
        
        private void Update()
        {
            if (_hasUpdated[0] & _hasUpdated[1] & _hasUpdated[2] & _hasUpdated[3])
            {
                UpdateMovement();
                _hasUpdated[0] = false;
                _hasUpdated[1] = false;
                _hasUpdated[2] = false;
                _hasUpdated[3] = false;
            }
        }
    }
}