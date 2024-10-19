using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System;
using UnityEngine.UIElements;

namespace LocomotionStateMachine
{

    public class JumpNode : LocomotionNode
    {
        public JumpNode(string stateName, string name, bool isEntry) : base(stateName, name, isEntry, true)
        {
            
        }
    }
}
