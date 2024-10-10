using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using System;
using UnityEngine.UIElements;

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

        // TODO: Test Graph
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
                container.BlockData = locomotionContainerObject.BlockData;
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
                        locomotionContainerObject.LocomotionNodeData.Add(new LocomotionNodeData { NodeGUID = guid, Position = position, LocomotionStateName = tmpLNode.StateName }); 
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
                var nodes = block.containedElements.Where(x => x is LocomotionNode).Cast<LocomotionNode>().Select(x => x.GUID)
                    .ToList();

                locomotionContainer.BlockData.Add(new BlockData
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
                EditorUtility.DisplayDialog("File Not Found", "Target Narrative Data does not exist!", "OK");
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
            Nodes.Find(x => x.EntyPoint).GUID = _locomotionContainer.NodeLinks[0].BaseNodeGUID;
            foreach (var perNode in Nodes)
            {
                if (perNode.EntyPoint) continue;
                Edges.Where(x => x.input.node == perNode).ToList()
                    .ForEach(edge => _graphView.RemoveElement(edge));
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
                
                var tmpNode = _locomotionContainer.LocomotionNodeData.Where(x => x.NodeGUID == guid).First();
                var tmpLNode = _graphView.CreateNode(tmpNode.LocomotionStateName, position);
                tmpLNode.GUID = guid;
                var nodePorts = _locomotionContainer.NodeLinks.Where(x => x.BaseNodeGUID == guid).ToList();
                foreach (var x in nodePorts)
                {
                    tmpLNode.AddOutputPort(x.BasePortName);
                }
                _graphView.AddElement(tmpLNode);
            }
            foreach (var perNode in _locomotionContainer.ConditionNodeData)
            {
                string guid = perNode.NodeGUID;
                Vector2 position = perNode.Position;
                if (string.IsNullOrEmpty(guid)) continue;

                var tmpNode = _locomotionContainer.ConditionNodeData.Where(x => x.NodeGUID == guid).First();
                var tempCNode = _graphView.CreateNode(tmpNode.Condition, position);
                tempCNode.GUID = guid;
                _graphView.AddElement(tempCNode);
            }
            foreach (var perNode in _locomotionContainer.TransitionNodeData)
            {
                string guid = perNode.NodeGUID;
                Vector2 position = perNode.Position;
                if (string.IsNullOrEmpty(guid)) continue;

                var tmpNode = _locomotionContainer.TransitionNodeData.Where(x => x.NodeGUID == guid).First();
                var tempTNode = _graphView.CreateNode(tmpNode.Operator, position);
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
            for (var i = 0; i < Nodes.Count; i++)
            {
                var k = i; //Prevent access to modified closure
                var connections = _locomotionContainer.NodeLinks.Where(x => x.BaseNodeGUID == Nodes[k].GUID).ToList();
                for (var j = 0; j < connections.Count(); j++)
                {
                    var targetNodeGUID = connections[j].TargetNodeGUID;
                    var targetNode = Nodes.First(x => x.GUID == targetNodeGUID);
                    Port targetPort = null;
                    for (int t = 0; t < targetNode.inputContainer.childCount; t++)
                    {
                        var port = (Port)targetNode.inputContainer[t];
                        if (port.portName == connections[j].TargetPortName)
                        {
                            targetPort = port;
                            break;
                        }
                    }
                    LinkNodesTogether((Port)Nodes[i].outputContainer[j], targetPort);

                    Vector2 position = Vector2.zero;
                    if (_locomotionContainer.LocomotionNodeData.Any(x => x.NodeGUID == targetNodeGUID))
                        position = _locomotionContainer.LocomotionNodeData.First(x => x.NodeGUID == targetNodeGUID).Position;
                    else if (_locomotionContainer.ConditionNodeData.Any(x => x.NodeGUID == targetNodeGUID))
                        position = _locomotionContainer.ConditionNodeData.First(x => x.NodeGUID == targetNodeGUID).Position;
                    else
                        position = _locomotionContainer.TransitionNodeData.First(x => x.NodeGUID == targetNodeGUID).Position;
                    targetNode.SetPosition(new Rect(position, _graphView.DefaultNodeSize));
                }
            }
        }

        private void LinkNodesTogether(Port outputSocket, Port inputSocket)
        {
            if(inputSocket == null)
            {
                Debug.LogWarning(outputSocket.userData + " is null");
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

            foreach (var commentBlockData in _locomotionContainer.BlockData)
            {
                var block = _graphView.CreateBlock(new Rect(commentBlockData.Position, _graphView.DefaultBlockSize),
                     commentBlockData);
                block.AddElements(Nodes.Where(x => commentBlockData.ChildNodes.Contains(x.GUID)));
            }
        }
    }
}
