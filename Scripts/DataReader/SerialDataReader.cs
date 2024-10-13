using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using TMPro;
using UnityEngine;

namespace LocomotionStateMachine
{
    public class SerialDataReader : MonoBehaviour, IReader
    {
        [Header("Please check before you running the program.")]
        public string COMPort;
        public bool Stop = false;
        private SerialPort _mainSerialPort;
        private int _bandrate = 115200;
        private float _initClock = 0.0f;
        Thread myThread;

        // filter
        public static readonly Queue<Action> _executionQueue = new Queue<Action>();

        public static Action<DeviceType, int> OnValueChange;
        private void Enqueue(Action action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }
        
        private void OnEnable()
        {
            Invoke("Init", _initClock);
        }            
        private void Update()
        {
            if (Stop)
                return;
            lock (_executionQueue)
            {
                while (_executionQueue.Count > 0)
                {
                    _executionQueue.Dequeue()?.Invoke();
                }
            }
        }
        public void Init()
        {
            myThread = new Thread(new ThreadStart(Read));
            myThread.Start();
        }
        public void Read()
        {
            
            try
            {
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
                    if (Stop)
                        return;
                    string[] getCommandLine = _mainSerialPort.ReadLine().Split(',');
                    char foot = char.Parse(getCommandLine[0]);
                    int toe = int.Parse(getCommandLine[1]);
                    int heel = int.Parse(getCommandLine[2]);
                    if (foot == 'R')
                    {
                        _executionQueue.Enqueue(() => OnValueChange?.Invoke(DeviceType.RightToe, toe));
                        _executionQueue.Enqueue(() => OnValueChange?.Invoke(DeviceType.RightHeel, heel));
                        //OnValueChange?.Invoke(DeviceType.RightToe, ball);
                        //OnValueChange?.Invoke(DeviceType.RightToe, heel);
                    }
                    else
                    {
                        _executionQueue.Enqueue(() => OnValueChange?.Invoke(DeviceType.LeftToe, toe));
                        _executionQueue.Enqueue(() => OnValueChange?.Invoke(DeviceType.LeftHeel, heel));
                        //OnValueChange?.Invoke(DeviceType.LeftToe, ball);
                        //OnValueChange?.Invoke(DeviceType.LeftToe, heel);
                    }
                }
                catch (InvalidCastException e)
                {
                    Debug.LogWarning(e.Message);
                }
            }
        }
        public void Quit()
        {
            _executionQueue.Clear();
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
