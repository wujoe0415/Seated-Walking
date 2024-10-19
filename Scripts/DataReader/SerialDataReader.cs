using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using System.Linq;

namespace LocomotionStateMachine
{
    public class SerialDataReader : MonoBehaviour, IReader
    {
        [Header("Please check before you running the program.")]
        public string COMPort;
        public bool Stop = false;
        public int ToeOffset = 0;
        public int HeelOffset = 0;
        private SerialPort _mainSerialPort;
        private int _bandrate = 115200;
        private float _initClock = 1.2f;
        Thread myThread;
        public int Toe = 0;
        public int Heel = 0;
        private bool isLeft = false;

        private int _balancedNum = 100;
        // filter
        public static readonly Queue<Action> _executionQueue = new Queue<Action>();

        public static Action<DeviceType, int> OnValueChange;
        //private IEnumerator ArduinoInput;
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
            if(isLeft)
            {
                OnValueChange?.Invoke(DeviceType.LeftToe, Toe);
                OnValueChange?.Invoke(DeviceType.LeftHeel, Heel);
            }
            else
            {
                OnValueChange?.Invoke(DeviceType.RightToe, Toe);
                OnValueChange?.Invoke(DeviceType.RightHeel, Heel);
            }
            //lock (_executionQueue)
            //{
            //    while (_executionQueue.Count > 0)
            //    {
            //        _executionQueue.Dequeue()?.Invoke();
            //    }
            //}
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
            List<int> toeValues = new List<int>();
            List<int> heelValues = new List<int>();
            while (myThread.IsAlive && _mainSerialPort.IsOpen)        // if serial is open then constantly read the line
            {
                try
                {
                    if (Stop)
                        return;
                    string[] getCommandLine = _mainSerialPort.ReadLine().Split(',');
                    //foreach (string s in getCommandLine)
                    //    Debug.Log(s);
                    char foot = char.Parse(getCommandLine[0]);
                    int toe = int.Parse(getCommandLine[1]) - ToeOffset;
                    int heel = int.Parse(getCommandLine[2]) - HeelOffset;
                    Toe = toe > 0 ? toe : 0;
                    Heel = heel > 0 ? heel : 0;
                    if (foot != 'R')
                        isLeft = true;
                    if(toeValues.Count < _balancedNum)
                    {
                        toeValues.Add(toe);
                        heelValues.Add(heel);
                    }
                    else
                    {
                        ToeOffset = toeValues.Sum(x => Convert.ToInt32(x)) / _balancedNum;
                        HeelOffset = heelValues.Sum(x => Convert.ToInt32(x)) / _balancedNum;
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
