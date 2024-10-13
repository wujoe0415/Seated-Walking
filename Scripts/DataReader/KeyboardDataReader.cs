using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocomotionStateMachine
{
    public class KeyboardDataReader : MonoBehaviour, IReader
    {
        public bool Stop = false;
        public static Action<DeviceType, int> OnValueChange;
        public bool isLeft = true;
        public KeyCode Toe = KeyCode.Q;
        public KeyCode Heel = KeyCode.A;

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

            if (Input.GetKey(Toe))
                OnValueChange?.Invoke(isLeft? DeviceType.LeftToe: DeviceType.RightToe, 60);
            else
                OnValueChange?.Invoke(isLeft ? DeviceType.LeftToe : DeviceType.RightToe, 800);
            if (Input.GetKey(Heel))
                OnValueChange?.Invoke(isLeft ? DeviceType.LeftHeel : DeviceType.RightHeel, 60);
            else
                OnValueChange?.Invoke(isLeft ? DeviceType.LeftHeel : DeviceType.RightHeel, 800);
        }
        public void Quit()
        {
            
        }
    }
}
