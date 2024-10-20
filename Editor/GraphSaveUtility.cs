using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;

namespace LocomotionStateMachine
{
    public class GraphSaveUtility
    {
        private List<Edge> Edges => _graphView.edges.ToList();
        private List<BasicNode> Nodes => _graphView.nodes.ToList().Cast<BasicNode>().ToList();
        // Cast to conditionNode

        private List<Group> CommentBlocks =>
            _graphView.graphElements.ToList().Where(x => x is Group).Cast<Group>().ToList();

        private StateMachineContainer _locomotionContainer;
        private LocomotionGraphView _graphView;

        public static GraphSaveUtility GetInstance(LocomotionGraphView graphView)
        {
            return new GraphSaveUtility
            {
                _graphView = graphView
            };
        }

        public void SaveGraph(string fileName)
        {
            var locomotionContainerObject = ScriptableObject.CreateInstance<StateMachineContainer>();
            if (!SaveNodes(fileName, locomotionContainerObject)) return;
            SaveExposedProperties(locomotionContainerObject);
            SaveCommentBlocks(locomotionContainerObject);

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            UnityEngine.Object loadedAsset = AssetDatabase.LoadAssetAtPath($"Assets/Resources/{fileName}.asset", typeof(StateMachineContainer));

            if (loadedAsset == null || !AssetDatabase.Contains(loadedAsset))
            {
                AssetDatabase.CreateAsset(locomotionContainerObject, $"Assets/Resources/{fileName}.asset");
            }
            else
            {
                StateMachineContainer container = loadedAsset as StateMachineContainer;
                container.NodeLinks = locomotionContainerObject.NodeLinks;
                container.LocomotionNodeData = locomotionContainerObject.LocomotionNodeData;
                container.ConditionNodeData = locomotionContainerObject.ConditionNodeData;
                container.TransitionNodeData = locomotionContainerObject.TransitionNodeData;
                container.ExposedProperties = locomotionContainerObject.ExposedProperties;
                container.GroupDatas = locomotionContainerObject.GroupDatas;
                EditorUtility.SetDirty(container);
            }

            AssetDatabase.SaveAssets();
        }

        private bool SaveNodes(string fileName, StateMachineContainer locomotionContainerObject)
        {
            if (!Edges.Any()) return false;
            var connectedSockets = Edges.Where(x => x.input.node != null).ToArray();
            for (var i = 0; i < connectedSockets.Count(); i++)
            {
                var outputNode = (connectedSockets[i].output.node as BasicNode);
                var inputNode = (connectedSockets[i].input.node as BasicNode);
                locomotionContainerObject.NodeLinks.Add(new NodeLinkData
                {
                    BaseNodeGUID = outputNode.GUID,
                    BasePortName = connectedSockets[i].output.portName,
                    TargetPortName = connectedSockets[i].input.portName,
                    TargetNodeGUID = inputNode.GUID
                });
            }

            foreach (var node in Nodes.Where(node => !node.EntyPoint))
            {
                // determine if node is a condition node or locomotion node
                switch (node.Type)
                {
                    case BasicNode.NodeType.Condition:
                        var tmpCNode = node as ConditionNode;
                        string guid = tmpCNode.GUID;
                        Vector2 position = tmpCNode.GetPosition().position;
                        locomotionContainerObject.ConditionNodeData.Add(new ConditionNodeData { NodeGUID = guid, Position = position, Condition = tmpCNode.Condition });
                        break;
                    case BasicNode.NodeType.Transition:
                        var tmpTNode = node as TransitionNode;
                        guid = tmpTNode.GUID;
                        position = tmpTNode.GetPosition().position;
                        locomotionContainerObject.TransitionNodeData.Add(new TransitionNodeData { NodeGUID = guid, Position = position, Operator = tmpTNode.Operator});
                        break;
                    case BasicNode.NodeType.Locomotion:
                        var tmpLNode = node as LocomotionNode;
                        guid = tmpLNode.GUID;
                        position = tmpLNode.GetPosition().position;
                        locomotionContainerObject.LocomotionNodeData.Add(new LocomotionNodeData { NodeGUID = guid, Position = position, LocomotionStateName = tmpLNode.StateName}); 
                        break;
                }
            }
            return true;
        }

        private void SaveExposedProperties(StateMachineContainer locomotionContainer)
        {
            locomotionContainer.ExposedProperties.Clear();
            locomotionContainer.ExposedProperties.AddRange(_graphView.ExposedProperties);
        }

        private void SaveCommentBlocks(StateMachineContainer locomotionContainer)
        {
            foreach (var block in CommentBlocks)
            {
                var nodes = block.containedElements.Where(x => x is BasicNode).Cast<BasicNode>().Select(x => x.GUID)
                    .ToList();

                locomotionContainer.GroupDatas.Add(new GroupData
                {
                    ChildNodes = nodes,
                    Title = block.title,
                    Position = block.GetPosition().position
                });
            }
        }

        public void LoadGraph(string fileName)
        {
            _locomotionContainer = Resources.Load<StateMachineContainer>(fileName);
            if (_locomotionContainer == null)
            {
                EditorUtility.DisplayDialog("File Not Found", "Target File does not exist!", "OK");
                return;
            }

            ClearGraph();
            GenerateNodes();
            ConnectLocomotionNodes();
            AddExposedProperties();
            GenerateCommentBlocks();
        }

        /// <summary>
        /// Set Entry point GUID then Get All Nodes, remove all and their edges. Leave only the entrypoint node. (Remove its edge too)
        /// </summary>
        private void ClearGraph()
        {
            var rootNode = _locomotionContainer.NodeLinks.FirstOrDefault(x => x.BasePortName == "Root");
            if(rootNode != null)
                Nodes.Find(x => x.EntyPoint).GUID = rootNode.BaseNodeGUID;
            foreach (var perNode in Nodes)
            {
                if (perNode.EntyPoint) continue;
                Edges.Where(x => x.input.node == perNode).ToList()
                    .ForEach(edge => _graphView.RemoveElement(edge));
                _graphView.RemoveElement(perNode);
            }
            foreach(var perNode in CommentBlocks)
            {
                _graphView.RemoveElement(perNode);
            }
        }

        /// <summary>
        /// Create All serialized nodes and assign their guid and locomotion text to them
        /// </summary>
        private void GenerateNodes()
        {
            foreach (var perNode in _locomotionContainer.LocomotionNodeData)
            {
                string guid = perNode.NodeGUID;
                Vector2 position = perNode.Position;
                if (string.IsNullOrEmpty(guid)) continue;
                bool inputOnly = !(_locomotionContainer.NodeLinks.Any(x => x.BaseNodeGUID == guid));
                string title = inputOnly? $"Jump to {perNode.LocomotionStateName}" : perNode.LocomotionStateName;
                LocomotionNode tmpLNode = _graphView.CreateNode(title, perNode.LocomotionStateName, position, inputOnly);
                tmpLNode.GUID = guid;
                var nodePorts = _locomotionContainer.NodeLinks.Where(x => x.BaseNodeGUID == guid).ToList();
                foreach (var x in nodePorts)
                {
                    tmpLNode.AddOutputPort(x.BasePortName);
                }
                //tmpLNode.SetPosition(new Rect(position, _graphView.DefaultNodeSize));
                _graphView.AddElement(tmpLNode);
            }
            foreach (var perNode in _locomotionContainer.ConditionNodeData)
            {
                string guid = perNode.NodeGUID;
                Vector2 position = perNode.Position;
                if (string.IsNullOrEmpty(guid)) continue;
                var tempCNode = _graphView.CreateNode(perNode.Condition, position, typeof(ConditionNode));
                tempCNode.GUID = guid;
                _graphView.AddElement(tempCNode);
            }
            foreach (var perNode in _locomotionContainer.TransitionNodeData)
            {
                string guid = perNode.NodeGUID;
                Vector2 position = perNode.Position;
                if (string.IsNullOrEmpty(guid)) continue;

                var tempTNode = _graphView.CreateNode(perNode.Operator, position);
                tempTNode.GUID = guid;
                var nodePorts = _locomotionContainer.NodeLinks.Where(x => x.TargetNodeGUID == guid).ToList();
                foreach (var x in nodePorts)
                {
                    if(x.TargetPortName != "Input State")
                        tempTNode.AddInputPort(x.TargetPortName);
                }
                _graphView.AddElement(tempTNode);
            }
        }

        private void ConnectLocomotionNodes()
        {
            for (int i = 0; i < _locomotionContainer.NodeLinks.Count; i++)
            {
                var connection = _locomotionContainer.NodeLinks[i];
                BasicNode outputNode = Nodes.FirstOrDefault(x => x.GUID == connection.BaseNodeGUID);
                BasicNode inputNode = Nodes.FirstOrDefault(x => x.GUID == connection.TargetNodeGUID);
                Port outputPort = outputNode.outputContainer.Children().OfType<Port>().FirstOrDefault(x => x.portName == connection.BasePortName);
                Port inputPort = inputNode.inputContainer.Children().OfType<Port>().FirstOrDefault(x => x.portName == connection.TargetPortName);

                LinkNodesTogether(outputPort, inputPort);
            }
        }
        private void LinkNodesTogether(Port outputSocket, Port inputSocket)
        {
            if(outputSocket == null || inputSocket == null)
            {
                Debug.LogError($"Input socket: {inputSocket.userData}\nOutput socket: {outputSocket.userData}");
                return;
            }
            var tempEdge = new Edge()
            {
                output = outputSocket,
                input = inputSocket
            };
            tempEdge?.input.Connect(tempEdge);
            tempEdge?.output.Connect(tempEdge);
            _graphView.Add(tempEdge);
        }

        private void AddExposedProperties()
        {
            _graphView.ClearBlackBoardAndExposedProperties();
            foreach (var exposedProperty in _locomotionContainer.ExposedProperties)
            {
                _graphView.AddPropertyToBlackBoard(exposedProperty);
            }
        }

        private void GenerateCommentBlocks()
        {
            foreach (var commentBlock in CommentBlocks)
            {
                _graphView.RemoveElement(commentBlock);
            }
            
            foreach (var commentBlockData in _locomotionContainer.GroupDatas)
            {
                var block = _graphView.CreateGroup(new Rect(commentBlockData.Position, _graphView.DefaultBlockSize),
                     commentBlockData);
                block.AddElements(Nodes.Where(x => commentBlockData.ChildNodes.Contains(x.GUID)));
            }
        }
    }
}
