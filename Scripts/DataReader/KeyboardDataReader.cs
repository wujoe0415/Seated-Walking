using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocomotionStateMachine
{
    public class KeyboardDataReader : MonoBehaviour, IReader
    {
        public KeyCode LeftToe = KeyCode.Q;
        public KeyCode LeftHeel = KeyCode.A;
        public KeyCode RightToe = KeyCode.W;
        public KeyCode RightHeel = KeyCode.S;
        public bool Stop = false;
        public static Action<DeviceType, int> OnValueChange;

        private void Update()
        {
            Read();
        }
        public void Init()
        {
            
        }
        public void Read()
        {
            if (Stop)
                return;

            if (Input.GetKey(LeftToe))
                OnValueChange?.Invoke(DeviceType.LeftToe, 10);
            else
                OnValueChange?.Invoke(DeviceType.LeftToe, 800);
            if (Input.GetKey(LeftHeel))
                OnValueChange?.Invoke(DeviceType.LeftHeel, 10);
            else
                OnValueChange?.Invoke(DeviceType.LeftHeel, 800);

            if (Input.GetKey(RightToe))
                OnValueChange?.Invoke(DeviceType.RightToe, 10);
            else
                OnValueChange?.Invoke(DeviceType.RightToe, 800);
            if (Input.GetKey(RightHeel))
                OnValueChange?.Invoke(DeviceType.RightHeel, 10);
            else
                OnValueChange?.Invoke(DeviceType.RightHeel, 800);
        }
        public void Quit()
        {
            
        }
    }
}
