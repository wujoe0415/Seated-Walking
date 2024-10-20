using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.Port;
using Button = UnityEngine.UIElements.Button;

namespace LocomotionStateMachine
{
    public class LocomotionGraphView : GraphView
    {
        public readonly Vector2 DefaultLocomotionNodeSize = new Vector2(200, 150);
        public readonly Vector2 DefaultTransitionNodeSize = new Vector2(150, 100);
        public readonly Vector2 DefaultConditionNodeSize = new Vector2(100, 50);
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
            this.AddManipulator(CreateGroupContextMenu());
            this.AddManipulator(RemoveGroupContextMenu());
            OnElementsDelete();

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            AddElement(GetEntryPointNodeInstance());
            AddSearchWindow(editorWindow);

            canPasteSerializedData += evt => true;
            serializeGraphElements += OnCopy;
            unserializeAndPaste += OnPaste;
        }
        private IManipulator CreateGroupContextMenu()
        {
            ContextualMenuManipulator groupMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add to Group", actionEvent => CreateGroup(new Rect(contentViewContainer.LocalToWorld(actionEvent.eventInfo.localMousePosition), DefaultBlockSize))));

            return groupMenuManipulator;
        }
        private IManipulator RemoveGroupContextMenu()
        {
            ContextualMenuManipulator groupMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Remove from Group", actionEvent => RemoveGroup()));

            return groupMenuManipulator;
        }
        private void OnElementsDelete()
        {
            deleteSelection = (operationName, askUser) =>
            {
                Type groupType = typeof(Group);
                Type edgeType = typeof(Edge);
                List<BasicNode> nodes = new List<BasicNode>();
                List<Edge> deleted_edges = new List<Edge>();
                List<Group> groups = new List<Group>();

                foreach (var selected in selection)
                {
                    if (selected is BasicNode node)
                        nodes.Add(node);
                    else if (selected is Edge edge)
                        deleted_edges.Add(edge);
                    else if (selected is Group group)
                        groups.Add(group);
                }
                foreach (Group group in groups)
                {
                    List<BasicNode> basicNodes = group.containedElements.OfType<BasicNode>().ToList();
                    group.RemoveElements(basicNodes);
                }
                DeleteElements(deleted_edges);
                foreach (BasicNode node in nodes)
                {
                    var outputEdges = edges.ToList().Where(x => x.output.node == node);
                    var inputEdges = edges.ToList().Where(x => x.input.node == node);
                    foreach (Edge edge in outputEdges)
                    {
                        edge.input.Disconnect(edge);
                        RemoveElement(edge);
                    }
                    foreach (Edge edge in inputEdges)
                    {
                        edge.output.Disconnect(edge);
                        RemoveElement(edge);
                    }
                }
                DeleteElements(nodes);
                DeleteElements(groups);
            };

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
            if(Blackboard != null)
                Blackboard.Clear();
        }
        public Group CreateGroup(Rect rect, GroupData blockData = null)
        {
            if (blockData == null)
                blockData = new GroupData();
            Group group = new Group
            {
                autoUpdateGeometry = true,
                title = blockData.Title
            };
            foreach(GraphElement selected in selection)
            {
                if(selected is Node node)
                    group.AddElement(node);
            }
            AddElement(group);
            group.SetPosition(rect);
            return group;
        }
        public void RemoveGroup()
        {
            foreach (GraphElement selected in selection)
            {
                // if node has group, remove it from the group
                if (selected is not BasicNode node)
                    continue;
                List<Group> groups = graphElements.ToList().Where(x => x is Group).Cast<Group>().ToList();
                foreach(Group group in groups)
                {
                   if (group.containedElements.Contains(node))
                    {
                        group.RemoveElement(node);
                        break;
                    }
                }
            }
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
            AddElement(CreateNode(nodeName, nodeName, position));
        }
        public void CreateNewJumpNode(string nodeTitle, string nodeName, Vector2 position)
        {
            AddElement(CreateNode(nodeTitle, nodeName, position, true));
        }
        public void CreateNewConditionNode(string nodeName, StateCondition condition, Vector2 position, Type type)
        {
            AddElement(CreateNode(condition, position, type));
        }
        public void CreateNewTransitionNode(string nodeName, StateTransition.BooleanOperator transitionOperator, Vector2 position)
        {
            AddElement(CreateNode(transitionOperator, position));
        }
        public LocomotionNode CreateNode(string nodeTitle, string nodeName, Vector2 position, bool inputOnly = false)
        {
            LocomotionNode tempLocomotionNode = new LocomotionNode(nodeTitle, nodeName, false, inputOnly);
            tempLocomotionNode.RefreshExpandedState();
            tempLocomotionNode.RefreshPorts();
            tempLocomotionNode.SetPosition(new Rect(position, DefaultLocomotionNodeSize));
            if (!inputOnly)
            {
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
            }
            return tempLocomotionNode;
        }
        public ConditionNode CreateNode(StateCondition condition, Vector2 position, Type type)
        {
            if (condition.Duration > 0)
                type = typeof(HoldStillNode);
            else if (condition.LastDeviceState != HistoryState.Any)
                type = typeof(HistoryRecordNode);
            else if (!condition.TriggerMovement.isAllEverything())
                type = typeof(TriggerNode);

            // identify the type of the condition, it may be ConditionNode or TriggerNode
            var tempConditionNode = type == typeof(ConditionNode)
                                    ? new ConditionNode(condition, false): type == typeof(TriggerNode)
                                    ? new TriggerNode(condition, false): type == typeof(HistoryRecordNode)
                                    ? new HistoryRecordNode(condition, false):
                                    new HoldStillNode(condition, false);

            //ConditionNode tempConditionNode = new ConditionNode(condition, false);
            //tempConditionNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
            tempConditionNode.RefreshExpandedState();
            tempConditionNode.RefreshPorts();
            tempConditionNode.SetPosition(new Rect(position, DefaultConditionNodeSize));
            return tempConditionNode;
        }
        public TransitionNode CreateNode(StateTransition.BooleanOperator transitionOperator, Vector2 position)
        {
            TransitionNode tempTransitionNode = new TransitionNode(transitionOperator, false);
            //tempTransitionNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
            tempTransitionNode.SetPosition(new Rect(position, DefaultTransitionNodeSize));
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
            LocomotionNode node = new LocomotionNode("ENTRY", "ENTRYPOINT", true, false);
            var generatedPort = GetPortInstance(node, Direction.Output);
            generatedPort.portName = "Root";
            generatedPort.portColor = new Color(0.3f, 0.8f, 0.4f, 0.9f);
            node.outputContainer.Add(generatedPort);

            node.capabilities &= ~Capabilities.Movable;
            node.capabilities &= ~Capabilities.Deletable;

            node.RefreshExpandedState();
            node.RefreshPorts();
            
            node.SetPosition(new Rect(x: 100, y: 200, width: 100, height:150));
            return node;
        }
        public string CopyCache = "";
        
        public string OnCopy(IEnumerable<GraphElement> elements)
        {
            CopyCache = "";
            StateMachineContainer tempContainer = ScriptableObject.CreateInstance<StateMachineContainer>();
            foreach (GraphElement element in elements)
            {
                BasicNode node = element as BasicNode;
                // Save Nodes
                if(node != null)
                {
                    // determine if node is a condition node or locomotion node
                    switch (node.Type)
                    {
                        case BasicNode.NodeType.Condition:
                            var tmpCNode = node as ConditionNode;
                            string guid = tmpCNode.GUID;
                            Vector2 position = tmpCNode.GetPosition().position;
                            tempContainer.ConditionNodeData.Add(new ConditionNodeData { NodeGUID = guid, Position = position, Condition = tmpCNode.Condition });
                            break;
                        case BasicNode.NodeType.Transition:
                            var tmpTNode = node as TransitionNode;
                            guid = tmpTNode.GUID;
                            position = tmpTNode.GetPosition().position;
                            tempContainer.TransitionNodeData.Add(new TransitionNodeData { NodeGUID = guid, Position = position, Operator = tmpTNode.Operator });
                            break;
                        case BasicNode.NodeType.Locomotion:
                            var tmpLNode = node as LocomotionNode;
                            guid = tmpLNode.GUID;
                            position = tmpLNode.GetPosition().position;
                            tempContainer.LocomotionNodeData.Add(new LocomotionNodeData { NodeGUID = guid, Position = position, LocomotionStateName = tmpLNode.StateName });
                            break;
                    }
                    continue;
                }
                Edge connectedSockets = element as Edge;
                if(connectedSockets != null)
                {
                    // Save Edges
                    var outputNode = (connectedSockets.output.node as BasicNode);
                    var inputNode = (connectedSockets.input.node as BasicNode);
                    tempContainer.NodeLinks.Add(new NodeLinkData
                    {
                        BaseNodeGUID = outputNode.GUID,
                        BasePortName = connectedSockets.output.portName,
                        TargetPortName = connectedSockets.input.portName,
                        TargetNodeGUID = inputNode.GUID
                    });
                    continue;
                }

                Debug.LogError("Other data but not store in the graph!");
            }

            CopyCache = JsonUtility.ToJson(tempContainer);
            // store data in CopyCache
            return CopyCache;
        }
        public void OnPaste(string operationName, string data)
        {
            if (operationName != "Paste")
                return;

            ClearSelection();
            StateMachineContainer tempContainer = ScriptableObject.CreateInstance<StateMachineContainer>();
            JsonUtility.FromJsonOverwrite(data, tempContainer);
            // new -> origin
            Dictionary<string, string> guidMapper = new Dictionary<string, string>();
            List<BasicNode> nodes = new List<BasicNode>();
            Vector2 offsetPosition = new Vector2(90f, 90f);

            foreach (var perNode in tempContainer.LocomotionNodeData)
            {
                string guid = perNode.NodeGUID;
                Vector2 position = perNode.Position + offsetPosition;
                if (string.IsNullOrEmpty(guid)) continue;
                bool inputOnly = !(tempContainer.NodeLinks.Any(x => x.BaseNodeGUID == guid));
                string title = inputOnly ? $"Jump to {perNode.LocomotionStateName}" : perNode.LocomotionStateName;
                LocomotionNode tmpLNode = CreateNode(title, perNode.LocomotionStateName, position, inputOnly);
                guidMapper.Add(guid, tmpLNode.GUID);
                var nodePorts = tempContainer.NodeLinks.Where(x => x.BaseNodeGUID == guid).ToList();
                foreach (var x in nodePorts)
                    tmpLNode.AddOutputPort(x.BasePortName);
                AddElement(tmpLNode);
                nodes.Add(tmpLNode as BasicNode);
            }
            foreach (var perNode in tempContainer.ConditionNodeData)
            {
                string guid = perNode.NodeGUID;
                Vector2 position = perNode.Position + offsetPosition;
                if (string.IsNullOrEmpty(guid)) continue;
                var tempCNode = CreateNode(perNode.Condition, position, typeof(ConditionNode));
                guidMapper.Add(guid, tempCNode.GUID); 
                AddElement(tempCNode);
                nodes.Add(tempCNode as BasicNode);
            }
            foreach (var perNode in tempContainer.TransitionNodeData)
            {
                string guid = perNode.NodeGUID;
                Vector2 position = perNode.Position + offsetPosition;
                if (string.IsNullOrEmpty(guid)) continue;

                var tempTNode = CreateNode(perNode.Operator, position);
                guidMapper.Add(guid, tempTNode.GUID);
                var nodePorts = tempContainer.NodeLinks.Where(x => x.TargetNodeGUID == guid).ToList();
                foreach (var x in nodePorts)
                {
                    if (x.TargetPortName != "Input State")
                        tempTNode.AddInputPort(x.TargetPortName);
                }
                AddElement(tempTNode);
                nodes.Add(tempTNode as BasicNode);
            }

            // Node Link
            foreach (var connection in tempContainer.NodeLinks)
            {
                if (!guidMapper.ContainsKey(connection.BaseNodeGUID) || !guidMapper.ContainsKey(connection.TargetNodeGUID))
                    continue;
                var outputNode = nodes.First(x => x.GUID == guidMapper[connection.BaseNodeGUID]);
                var inputNode = nodes.First(x => x.GUID == guidMapper[connection.TargetNodeGUID]);
                Port outputPort = outputNode.outputContainer.Children().OfType<Port>().FirstOrDefault(x => x.portName == connection.BasePortName);
                Port inputPort = inputNode.inputContainer.Children().OfType<Port>().FirstOrDefault(x => x.portName == connection.TargetPortName);
                var tempEdge = new Edge()
                {
                    output = outputPort,
                    input = inputPort
                };
                tempEdge?.input.Connect(tempEdge);
                tempEdge?.output.Connect(tempEdge);
                Add(tempEdge);
            }

            foreach (var node in nodes)
                AddToSelection(node);
        }
    }
}
