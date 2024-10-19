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
        public string LeftCOMPort="COM3";
        public string RightCOMPort = "COM4";
        public bool Stop = false;

        private Thread _serialThread;
        private SerialPort _leftSerialPort;
        private SerialPort _rightSerialPort;
        private int _bandrate = 115200;
        private float _initClock = 1.2f;

        private int _leftToe = 0;
        private int _leftHeel = 0;
        private int _rightToe = 0;
        private int _rightHeel = 0;

        private int _leftToeOffset = 0;
        private int _leftHeelOffset = 0;
        private int _rightToeOffset = 0;
        private int _rightHeelOffset = 0;
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
            OnValueChange?.Invoke(DeviceType.LeftToe, _leftToe);
            OnValueChange?.Invoke(DeviceType.LeftHeel, _leftHeel);
            
            OnValueChange?.Invoke(DeviceType.RightToe, _leftToe);
            OnValueChange?.Invoke(DeviceType.RightHeel, _leftHeel);
        }
        public void Init()
        {
            _serialThread = new Thread(new ThreadStart(Read));
            _serialThread.Start();
        }
        public void Read()
        {
            try
            {
                _leftSerialPort = new SerialPort(LeftCOMPort, _bandrate);
                _leftSerialPort.Open();
                _rightSerialPort = new SerialPort(RightCOMPort, _bandrate);
                _rightSerialPort.Open();
            }
            catch(InvalidCastException e)
            {
                Debug.LogWarning(e.Message);
            }
            while (_serialThread.IsAlive && _leftSerialPort.IsOpen && _rightSerialPort.IsOpen)        // if serial is open then constantly read the line
            {
                try
                {
                    if (Stop)
                        return;
                    ParseData(_leftSerialPort.ReadLine());
                    ParseData(_rightSerialPort.ReadLine());                    
                }
                catch (InvalidCastException e)
                {
                    Debug.LogWarning(e.Message);
                }
            }
        }
        private List<int> _leftToeValues = new List<int>();
        private List<int> _leftHeelValues = new List<int>();
        private List<int> _rightToeValues = new List<int>();
        private List<int> _rightHeelValues = new List<int>();

        private int _balancedNum = 100;
        private void ParseData(string line)
        {
            if (string.IsNullOrEmpty(line))
                return;
            string[] datas = line.Split(',');
            char foot = char.Parse(datas[0]);
            if (foot == 'R')
            {
                if (_rightToeValues.Count < _balancedNum)
                {
                    _rightToeValues.Add(int.Parse(datas[1]));
                    _rightHeelValues.Add(int.Parse(datas[2]));
                }
                else
                {
                    _rightToeOffset = _rightToeValues.Sum(x => Convert.ToInt32(x)) / _balancedNum;
                    _rightHeelOffset = _rightHeelValues.Sum(x => Convert.ToInt32(x)) / _balancedNum;
                }
                _rightToe = int.Parse(datas[1]) > _rightToeOffset ? int.Parse(datas[1]) - _rightToeOffset : 0;
                _rightHeel = int.Parse(datas[2]) > _rightHeelOffset ? int.Parse(datas[2]) - _rightHeelOffset : 0;
                return;
            }
            else if (foot == 'L')
            {
                if (_leftToeValues.Count < _balancedNum)
                {
                    _leftToeValues.Add(int.Parse(datas[1]));
                    _leftHeelValues.Add(int.Parse(datas[2]));
                }
                else
                {
                    _leftToeOffset = _leftToeValues.Sum(x => Convert.ToInt32(x)) / _balancedNum;
                    _leftHeelOffset = _leftHeelValues.Sum(x => Convert.ToInt32(x)) / _balancedNum;
                }

                _leftToe = int.Parse(datas[1]) > _leftToeOffset ? int.Parse(datas[1]) - _leftToeOffset : 0;
                _leftHeel = int.Parse(datas[2]) > _leftHeelOffset ? int.Parse(datas[2]) - _leftHeelOffset : 0;
            }
        }
        public void Quit()
        {
            _executionQueue.Clear();
            if (_leftSerialPort.IsOpen)
                _leftSerialPort.Close();
            if (_serialThread.IsAlive)
                _serialThread.Abort();
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
