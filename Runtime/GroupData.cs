using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocomotionStateMachine
{
    [SerializeField]
    [System.Serializable]
    public class GroupData
    {
        public List<string> ChildNodes = new List<string>();
        public Vector2 Position;
        public string Title = "New Group";
    }
}
