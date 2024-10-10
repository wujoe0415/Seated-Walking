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
                new SearchTreeGroupEntry(new GUIContent("Condition"), 1),
                new SearchTreeEntry(new GUIContent("Add Locomotion State", _indentationIcon)) { level = 1, userData = new LocomotionNode("Locomotion State", "Locomotion State", false)},
                new SearchTreeEntry(new GUIContent("Add Transition", _indentationIcon)) { level = 1, userData = new TransitionNode(StateTransition.BooleanOperator.AND, false)},
                new SearchTreeEntry(new GUIContent("Trigger Condition", _indentationIcon)) { level = 1, userData = new ConditionNode(new StateCondition(), false)},
                new SearchTreeEntry(new GUIContent("Hold Still Duration Condition", _indentationIcon)) { level = 1, userData = new ConditionNode(new StateCondition(), false)},
                new SearchTreeEntry(new GUIContent("Record Track Condition", _indentationIcon)) { level = 1, userData = new ConditionNode(new StateCondition(), false)},
                new SearchTreeEntry(new GUIContent("Create Block", _indentationIcon)) { level = 1, userData = new Group() }
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
                case LocomotionNode node:
                    _graphView.CreateNewLocomotionNode("Locomotion Node", graphMousePosition);
                    return true;
                case ConditionNode condition:
                    _graphView.CreateNewConditionNode("Condition Node", new StateCondition(), graphMousePosition);
                    return true;
                case TransitionNode transition:
                    _graphView.CreateNewTransitionNode("Transition Node", StateTransition.BooleanOperator.AND, graphMousePosition);
                    return true;
                case Group group:
                    var rect = new Rect(graphMousePosition, _graphView.DefaultBlockSize);
                    _graphView.CreateBlock(rect);
                    return true;
            }
            return false;
        }
    }
}
