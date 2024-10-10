using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

namespace LocomotionStateMachine
{
    public class SerialDataReader : DataReader
    {
        [Header("Please check before you running the program.")]
        public string COMPort;
        private SerialPort _mainSerialPort;
        private int _bandrate = 115200;
        private float _initClock = 0.0f;
        Thread myThread;
        
        // filter

        public static Action<DeviceType, int> OnValueChange;

        private void OnEnable()
        {
            Invoke("Init", _initClock);
            StartCoroutine(KeyboardSimulator());
        }
            
        private IEnumerator KeyboardSimulator()
        {
            while(true)
            {
                if (Input.GetKey(KeyCode.Q))
                    OnValueChange?.Invoke(DeviceType.LeftToe, 60);
                else
                    OnValueChange?.Invoke(DeviceType.LeftToe, 800);
                if (Input.GetKey(KeyCode.A))
                    OnValueChange?.Invoke(DeviceType.LeftHeel, 60);
                else
                    OnValueChange?.Invoke(DeviceType.LeftHeel, 800);
                if (Input.GetKey(KeyCode.W))
                    OnValueChange?.Invoke(DeviceType.RightToe, 60);
                else
                    OnValueChange?.Invoke(DeviceType.RightToe, 800);
                if (Input.GetKey(KeyCode.S))
                    OnValueChange?.Invoke(DeviceType.RightHeel, 60);
                else
                    OnValueChange?.Invoke(DeviceType.RightHeel, 800);
                yield return null;
            }
        }
        public override void Init()
        {
            myThread = new Thread(new ThreadStart(Read));
            myThread.Start();
        }
        public override void Read()
        {
            try{
                _mainSerialPort = new SerialPort(COMPort, _bandrate);
                _mainSerialPort.Open();
            }
            catch(InvalidCastException e)
            {
                Debug.LogWarning(e.Message);
            }
            while (myThread.IsAlive && _mainSerialPort.IsOpen)        // if serial is open then constantly read the line
            {
                try
                {
                    string[] getCommandLine = _mainSerialPort.ReadLine().Split(',');
                    char foot = char.Parse(getCommandLine[0]);
                    int ball = int.Parse(getCommandLine[1]);
                    int heel = int.Parse(getCommandLine[2]);
                    if (foot == 'R')
                    {
                        OnValueChange?.Invoke(DeviceType.RightToe, ball);
                        OnValueChange?.Invoke(DeviceType.RightToe, heel);
                    }
                    else
                    {
                        OnValueChange?.Invoke(DeviceType.LeftToe, ball);
                        OnValueChange?.Invoke(DeviceType.LeftToe, heel);
                    }
                }
                catch (InvalidCastException e)
                {
                    Debug.LogWarning(e.Message);
                }
            }
        }
        public override void Quit()
        {
            if (_mainSerialPort.IsOpen)
                _mainSerialPort.Close();
            if (myThread.IsAlive)
                myThread.Abort();
        }
        private void OnApplicationQuit()
        {
            Quit();
        }
        private void OnDisable()
        {
            Quit();
        }
    }
}
