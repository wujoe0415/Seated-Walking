using UnityEngine;

namespace LocomotionStateMachine {
    [System.Serializable]
    public enum HistoryState
    {
        Any,                    // Not care
        LastSteppedDevice,
        LastSteppedToe,
        LastSteppedHeel,
        LastSteppedLeftShoe,
        LastSteppedRightShoe, 
        LastRaiseDevice,
        LastRaisedToe,
        LastRaisedHeel,
        LastRaisedLeftShoe,
        LastRaisedRightShoe,
    }

    [System.Serializable]
    public class StateCondition
    {
        public MovementEnumerator TriggerMovement;
        public HistoryState LastDeviceState = HistoryState.Any;
        public DeviceType Device;
        public float Duration = 0f;
        public float _currentDuration = 0f;

        public StateCondition()
        {
            TriggerMovement = new MovementEnumerator();
            LastDeviceState = HistoryState.Any;
            Device = DeviceType.LeftToe;
            _currentDuration = 0f;
        }
        public void ResetCondition()
        {
            _currentDuration = 0f;
        }
        public bool IsSatisfied(MovementEnumerator c, bool isMoniter)
        {
            if (!isMoniter)
            {
                return isTrigger(c) & isMatchHistory();
            }
            else
            {
                return checkDuration(c) & isMatchHistory();
            }
        }
        private bool isMatch(MovementEnumerator m1)
        {
            bool[] flags = new bool[4];
            for (int i = 0; i < flags.Length; i++)
                flags[i] = false;

            if (TriggerMovement.LeftToe.HasFlag(m1.LeftToe))
                flags[0] = true;
            if (TriggerMovement.LeftHeel.HasFlag(m1.LeftHeel))
                flags[1] = true;
            if (TriggerMovement.RightToe.HasFlag(m1.RightToe))
                flags[2] = true;
            if (TriggerMovement.RightHeel.HasFlag(m1.RightHeel))
                flags[3] = true;
            return flags[0] & flags[1] & flags[2] & flags[3];
        }
        private bool isMatch(MovementEnumerator e, DeviceType d, Movement m)
        {
            if (d == DeviceType.LeftToe)
                return e.LeftToe.HasFlag(m);
            else if (d == DeviceType.RightToe)
                return e.RightToe.HasFlag(m);
            else if (d == DeviceType.LeftHeel)
                return e.LeftHeel.HasFlag(m);
            else
                return e.RightHeel.HasFlag(m);
        }
        private bool isTrigger(MovementEnumerator e)
        {
            if (Duration > 0)
            {
                return false;
            }
            return isMatch(e);
        }
        private bool checkDuration(MovementEnumerator e)
        {
            if (Duration == 0f)
                return false;
            if (isMatch(e))
            {
                if (_currentDuration > Duration)
                {
                    return true;
                }
                else
                    _currentDuration += Time.deltaTime;
            }
            else
                _currentDuration = 0f;
            return false;
        }
        private bool isMatchHistory()
        {
            if (LastDeviceState == HistoryState.Any)
                return true;

            switch (LastDeviceState)
            {
                case HistoryState.LastSteppedDevice:
                    return HistoryRecorder.s_LastStepDevice == Device;
                case HistoryState.LastSteppedToe:
                    return HistoryRecorder.s_LastSteppedToe == Device;
                case HistoryState.LastSteppedHeel:
                    return HistoryRecorder.s_LastSteppedHeel == Device;
                case HistoryState.LastSteppedLeftShoe:
                    return HistoryRecorder.s_LastSteppedLeftShoe == Device;
                case HistoryState.LastSteppedRightShoe:
                    return HistoryRecorder.s_LastSteppedRightShoe == Device;
                case HistoryState.LastRaiseDevice:
                    return HistoryRecorder.s_LastRaisedDevice == Device;
                case HistoryState.LastRaisedToe:
                    return HistoryRecorder.s_LastRaisedToe == Device;
                case HistoryState.LastRaisedHeel:
                    return HistoryRecorder.s_LastRaisedHeel == Device;
                case HistoryState.LastRaisedLeftShoe:
                    return HistoryRecorder.s_LastRaisedLeftShoe == Device;
                case HistoryState.LastRaisedRightShoe:
                    return HistoryRecorder.s_LastRaisedRightShoe == Device;
                default:
                    return false;
            }

        }
    }
}
