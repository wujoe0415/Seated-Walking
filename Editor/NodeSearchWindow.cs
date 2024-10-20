using Codice.Client.BaseCommands.BranchExplorer;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace LocomotionStateMachine
{
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private EditorWindow _window;
        private LocomotionGraphView _graphView;
        private Texture2D _indentationIcon;

        public void Configure(EditorWindow window, LocomotionGraphView graphView)
        {
            _window = window;
            _graphView = graphView;

            _indentationIcon = new Texture2D(1, 1);
            _indentationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            _indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
                new SearchTreeGroupEntry(new GUIContent("State"), 1),
                new SearchTreeEntry(new GUIContent("New Locomotion State", _indentationIcon)) { level = 2, userData = new LocomotionNode("Locomotion State", "Locomotion State", false, false)},
                new SearchTreeEntry(new GUIContent("Jump to State", _indentationIcon)) { level = 2, userData = new JumpNode("Jump to Locomotion State", "Locomotion State", false)},
                new SearchTreeEntry(new GUIContent("Transition", _indentationIcon)) { level = 1, userData = new TransitionNode(StateTransition.BooleanOperator.AND, false)},

                new SearchTreeGroupEntry(new GUIContent("Condition"), 1),
                new SearchTreeEntry(new GUIContent("Any", _indentationIcon)) { level = 2, userData = new ConditionNode(new StateCondition(), false)},
                new SearchTreeEntry(new GUIContent("Trigger", _indentationIcon)) { level = 2, userData = new TriggerNode(new StateCondition(), false)},
                new SearchTreeEntry(new GUIContent("Hold Still Trigger", _indentationIcon)) { level = 2, userData = new HoldStillNode(new StateCondition(), false)},
                new SearchTreeEntry(new GUIContent("Last State", _indentationIcon)) { level = 2, userData = new HistoryRecordNode(new StateCondition(), false)},
                new SearchTreeEntry(new GUIContent("Create Group", _indentationIcon)) { level = 1, userData = new Group() }
            };

            return tree;  
        }
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            //Editor window-based mouse position
            Vector2 mousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent,
                context.screenMousePosition - _window.position.position);
            var graphMousePosition = _graphView.contentViewContainer.WorldToLocal(mousePosition);
            switch (searchTreeEntry.userData)
            {
                case JumpNode node:
                    _graphView.CreateNewJumpNode("Jump to State Node", "State Node", graphMousePosition);
                    return true;
                case LocomotionNode node:
                    _graphView.CreateNewLocomotionNode("Locomotion Node", graphMousePosition);
                    return true;
                case HistoryRecordNode condition:
                    _graphView.CreateNewConditionNode("History Record Node", new StateCondition(), graphMousePosition, typeof(HistoryRecordNode));
                    return true;
                case TriggerNode condition:
                    _graphView.CreateNewConditionNode("Trigger Node", new StateCondition(), graphMousePosition, typeof(TriggerNode));
                    return true;
                case HoldStillNode condition:
                    _graphView.CreateNewConditionNode("Hold Still Node", new StateCondition(), graphMousePosition, typeof(HoldStillNode));
                    return true;
                case ConditionNode condition:
                    _graphView.CreateNewConditionNode("Condition Node", new StateCondition(), graphMousePosition, typeof(ConditionNode));
                    return true;
                case TransitionNode transition:
                    _graphView.CreateNewTransitionNode("Transition Node", StateTransition.BooleanOperator.AND, graphMousePosition);
                    return true;
                case Group group:
                    var rect = new Rect(graphMousePosition, _graphView.DefaultBlockSize);
                    _graphView.CreateGroup(rect);
                    return true;
            }
            return false;
        }
    }
}
