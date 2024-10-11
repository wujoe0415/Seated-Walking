using Codice.CM.SEIDInfo;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.Port;
using Button = UnityEngine.UIElements.Button;

namespace LocomotionStateMachine
{
    public class LocomotionGraphView : GraphView
    {
        public readonly Vector2 DefaultNodeSize = new Vector2(200, 150);
        public readonly Vector2 DefaultBlockSize = new Vector2(150, 100);

        public LocomotionNode EntryNode;
        public Blackboard Blackboard;
        public List<ExposedProperty> ExposedProperties = new List<ExposedProperty>();
        private NodeSearchWindow _searchWindow;

        public LocomotionGraphView(LocomotionGraph editorWindow)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("LocomotionStateMachine"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            AddElement(GetEntryPointNodeInstance());

            AddSearchWindow(editorWindow);
        }
        private void AddSearchWindow(LocomotionGraph editorWindow)
        {
            _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            _searchWindow.Configure(editorWindow, this);
            nodeCreationRequest = context =>
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }
        public void ClearBlackBoardAndExposedProperties()
        {
            ExposedProperties.Clear();
            Blackboard.Clear();
        }
        public Group CreateBlock(Rect rect, BlockData blockData = null)
        {
            if (blockData == null)
                blockData = new BlockData();
            var group = new Group
            {
                autoUpdateGeometry = true,
                title = blockData.Title
            };
            AddElement(group);
            group.SetPosition(rect);
            return group;
        }
        public void AddPropertyToBlackBoard(ExposedProperty property, bool loadMode = false)
        {
            var localPropertyName = property.PropertyName;
            var localPropertyValue = property.PropertyValue;
            if (!loadMode)
            {
                while (ExposedProperties.Any(x => x.PropertyName == localPropertyName))
                    localPropertyName = $"{localPropertyName}(1)";
            }

            var item = ExposedProperty.CreateInstance();
            item.PropertyName = localPropertyName;
            item.PropertyValue = localPropertyValue;
            ExposedProperties.Add(item);

            var container = new VisualElement();
            var field = new BlackboardField { text = localPropertyName, typeText = "string" };
            container.Add(field);

            var propertyValueTextField = new TextField("Value:")
            {
                value = localPropertyValue
            };
            propertyValueTextField.RegisterValueChangedCallback(evt =>
            {
                var index = ExposedProperties.FindIndex(x => x.PropertyName == item.PropertyName);
                ExposedProperties[index].PropertyValue = evt.newValue;
            });
            var sa = new BlackboardRow(field, propertyValueTextField);
            container.Add(sa);
            Blackboard.Add(container);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            var startPortView = startPort;

            ports.ForEach((port) =>
            {
                var portView = port;
                if (startPortView != portView && startPortView.node != portView.node)
                    compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }
        public void CreateNewLocomotionNode(string nodeName, Vector2 position)
        {
            AddElement(CreateNode(nodeName, position));
        }
        public void CreateNewConditionNode(string nodeName, StateCondition condition, Vector2 position, Type type)
        {
            AddElement(CreateNode(condition, position, type));
        }
        public void CreateNewTransitionNode(string nodeName, StateTransition.BooleanOperator transitionOperator, Vector2 position)
        {
            AddElement(CreateNode(transitionOperator, position));
        }
        public LocomotionNode CreateNode(string nodeName, Vector2 position)
        {
            LocomotionNode tempLocomotionNode = new LocomotionNode(nodeName, nodeName, false);
            tempLocomotionNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
            tempLocomotionNode.RefreshExpandedState();
            tempLocomotionNode.RefreshPorts();
            tempLocomotionNode.SetPosition(new Rect(position, DefaultNodeSize));
            tempLocomotionNode.OnDeletePort += (node, port) =>
            {
                var targetEdge = edges.ToList()
                    .Where(x => x.output.portName == port.portName && x.output.node == port.node);
                if (targetEdge.Any())
                {
                    var edge = targetEdge.First();
                    edge.output.Disconnect(edge);
                    RemoveElement(targetEdge.First());
                }

                node.outputContainer.Remove(port);
                node.RefreshPorts();
                node.RefreshExpandedState();
            };
            return tempLocomotionNode;
        }
        public ConditionNode CreateNode(StateCondition condition, Vector2 position, Type type)
        {
            // identify the type of the condition, it may be ConditionNode or TriggerNode
            var tempConditionNode = type == typeof(ConditionNode)
                                    ? new ConditionNode(condition, false): type == typeof(TriggerNode)
                                    ? new TriggerNode(condition, false): 
                                    new HistoryRecordNode(condition, false);

            //ConditionNode tempConditionNode = new ConditionNode(condition, false);
            tempConditionNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
            tempConditionNode.RefreshExpandedState();
            tempConditionNode.RefreshPorts();
            tempConditionNode.SetPosition(new Rect(position, DefaultNodeSize));
            return tempConditionNode;
        }
        public TransitionNode CreateNode(StateTransition.BooleanOperator transitionOperator, Vector2 position)
        {
            TransitionNode tempTransitionNode = new TransitionNode(transitionOperator, false);
            tempTransitionNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
            tempTransitionNode.SetPosition(new Rect(position, DefaultBlockSize));
            tempTransitionNode.OnDeletePort += (node, port) =>
            {
                var targetEdge = edges.ToList()
                .Where(x => x.output.portName == port.portName && x.output.node == port.node);
                if (targetEdge.Any())
                {
                    var edge = targetEdge.First();
                    edge.input.Disconnect(edge);
                    RemoveElement(targetEdge.First());
                }
                node.inputContainer.Remove(port);
                node.RefreshPorts();
                node.RefreshExpandedState();
            };
            return tempTransitionNode;
        }
        private Port GetPortInstance(BasicNode node, Direction nodeDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, nodeDirection, capacity, typeof(string));
        }
        private LocomotionNode GetEntryPointNodeInstance()
        {
            LocomotionNode node = new LocomotionNode("ENTRY", "ENTRYPOINT", true);

            var generatedPort = GetPortInstance(node, Direction.Output);
            generatedPort.portName = "Root";
            node.outputContainer.Add(generatedPort);

            node.capabilities &= ~Capabilities.Movable;
            node.capabilities &= ~Capabilities.Deletable;

            node.RefreshExpandedState();
            node.RefreshPorts();
            
            node.SetPosition(new Rect(x: 100, y: 200, width: 100, height:150));
            return node;
        }
    }
}
