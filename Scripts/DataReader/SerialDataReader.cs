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

        public List<int> footSGdata = new List<int>();
        private float _initClock = 0.0f;
        Thread myThread;

        public static Action<StepType, int> OnValueChange;
        
        private void OnEnable()
        {
            Invoke("Init", _initClock);
        }
        public override void Init()
        {
            _mainSerialPort = new SerialPort(COMPort, _bandrate);
            _mainSerialPort.Open();
            myThread = new Thread(new ThreadStart(Read));
            myThread.Start();
        }

        public override void Read()
        {
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
                        OnValueChange?.Invoke(StepType.RightShoeBall, ball);
                        OnValueChange?.Invoke(StepType.RightShoeBall, heel);
                    }
                    else
                    {
                        OnValueChange?.Invoke(StepType.LeftShoeBall, ball);
                        OnValueChange?.Invoke(StepType.LeftShoeBall, heel);
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
