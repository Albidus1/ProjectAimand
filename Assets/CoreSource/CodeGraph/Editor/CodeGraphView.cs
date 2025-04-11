using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;



namespace CodeGraph.Editor
{
    public class CodeGraphView : GraphView
    {
        public CodeGraphEditorWindow window => m_window;
        public List<CodeGraphEditorNode> graphNodes;
        public Dictionary<string, CodeGraphEditorNode> nodeDictionary;
        public Dictionary<Edge, CodeGraphConnection> connectionDictionary;

        private CodeGraphAsset m_codeGraph;
        private SerializedObject m_serializedObject;
        private CodeGraphEditorWindow m_window;

        private CodeGraphWinodwSearchProvider m_searchProvider;



        public CodeGraphView(SerializedObject _serializedObject, CodeGraphEditorWindow _window)  
        {
            m_serializedObject = _serializedObject;
            m_codeGraph = (CodeGraphAsset)m_serializedObject.targetObject;
            m_window = _window;

            graphNodes = new List<CodeGraphEditorNode>();
            nodeDictionary = new Dictionary<string, CodeGraphEditorNode>();
            connectionDictionary = new Dictionary<Edge, CodeGraphConnection>();

            m_searchProvider = ScriptableObject.CreateInstance<CodeGraphWinodwSearchProvider>();
            m_searchProvider.graph = this;

            this.nodeCreationRequest = ShowSearchWindow;

            StyleSheet style = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/CoreSource/CodeGraph/Editor/USS/CodeGraphEditor.uss"); 
            styleSheets.Add(style);

            GridBackground backGround = new GridBackground();
            backGround.name = "Grid";
            Add(backGround);
            backGround.SendToBack();

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());

            DrawNodes();
            DrawConnections();

            graphViewChanged += OnGraphViewChangeEvent;
        }

        public override List<Port> GetCompatiblePorts(Port _startPort, NodeAdapter _nodeAdapter)
        {
            List<Port> allPorts = new List<Port>();
            List<Port> ports = new List<Port>();

            foreach (CodeGraphEditorNode node in graphNodes)
            {
                allPorts.AddRange(node.ports);
            }

            foreach (Port p in allPorts)
            {
                if (p == _startPort)
                    continue;
                if (p.node == _startPort.node)
                    continue;
                if (p.direction == _startPort.direction)
                    continue;
                if (p.portType == _startPort.portType)
                {
                    ports.Add(p);
                }
            }

            return ports;
        }

        private GraphViewChange OnGraphViewChangeEvent(GraphViewChange _graphViewChange)
        {
            if (_graphViewChange.movedElements != null)
            {
                Undo.RecordObject(m_serializedObject.targetObject, "Moved Elements");

                foreach (CodeGraphEditorNode editorNode in _graphViewChange.movedElements.OfType<CodeGraphEditorNode>())
                {
                    editorNode.SavePosition();
                }
            }

            if (_graphViewChange.elementsToRemove != null)
            {
                Undo.RecordObject(m_serializedObject.targetObject, "Removed Node");
                List<CodeGraphEditorNode> nodes = _graphViewChange.elementsToRemove.OfType<CodeGraphEditorNode>().ToList();
                //Debug.Log("노드 삭제 " + nodes.Count);

                if (nodes.Count > 0)
                {
                    for (int i = nodes.Count - 1; i >= 0; i--)
                    {
                        RemoveNodes(nodes[i]);
                    }

                    foreach (Edge e in _graphViewChange.elementsToRemove.OfType<Edge>())
                    {
                        RemoveConnection(e);
                    }
                }
            }

            if (_graphViewChange.edgesToCreate != null)
            {
                foreach (Edge edge in _graphViewChange.edgesToCreate)
                {
                    CreateEdge(edge);
                }
            }

            return _graphViewChange;
        }

        private void CreateEdge(Edge _edge)
        {
            CodeGraphEditorNode inputNode = (CodeGraphEditorNode)_edge.input.node;
            int inputIndex = inputNode.ports.IndexOf(_edge.input);

            CodeGraphEditorNode outputNode = (CodeGraphEditorNode)_edge.output.node;
            int outputIndex = outputNode.ports.IndexOf(_edge.output);

            CodeGraphConnection connection = new CodeGraphConnection(inputNode.node.id, inputIndex, outputNode.node.id, outputIndex);
            m_codeGraph.connections.Add(connection);
        }

        private void DrawNodes()
        {
            foreach (CodeGraphNode node in m_codeGraph.nodes)
            {
                AddNodeToGraph(node);
            }

            Bind();
        }

        private void DrawConnections()
        {
            if (m_codeGraph.connections == null)
            {
                return;
            }    

            foreach (CodeGraphConnection connection in m_codeGraph.connections)
            {
                DrawConnection(connection);
            }
        }

        private void DrawConnection(CodeGraphConnection _connection)
        {
            CodeGraphEditorNode inputNode = GetNode(_connection.inputPort.nodeId);
            CodeGraphEditorNode outputNode = GetNode(_connection.outputPort.nodeId);

            if (inputNode == null || outputNode == null)
            {
                return;
            }

            Port inPort = inputNode.ports[_connection.inputPort.portIndex];
            Port outPort = outputNode.ports[_connection.outputPort.portIndex];
            Edge edge = inPort.ConnectTo(outPort);
            
            AddElement(edge);

            connectionDictionary.Add(edge, _connection);
        }

        private CodeGraphEditorNode GetNode(string _nodeId)
        {
            CodeGraphEditorNode node = null;
            nodeDictionary.TryGetValue(_nodeId, out node);

            return node;
        }

        private void RemoveNodes(CodeGraphEditorNode _editorNode)
        {
            m_codeGraph.nodes.Remove(_editorNode.node);

            nodeDictionary.Remove(_editorNode.node.id);
            graphNodes.Remove(_editorNode);

            m_serializedObject.Update();
        }

        private void RemoveConnection(Edge _edge)
        {
            if (connectionDictionary.TryGetValue(_edge, out CodeGraphConnection _connection))
            {
                m_codeGraph.connections.Remove(_connection);
                connectionDictionary.Remove(_edge);
            }
        }

        private void ShowSearchWindow(NodeCreationContext _context)
        {
            m_searchProvider.target = (VisualElement)focusController.focusedElement;
            SearchWindow.Open(new SearchWindowContext(_context.screenMousePosition), m_searchProvider);
        }

        public void Add(CodeGraphNode _node)
        {
            Undo.RecordObject(m_serializedObject.targetObject, "Added Node");

            m_codeGraph.nodes.Add(_node);
            m_serializedObject.Update();

            AddNodeToGraph(_node);
            Bind();
        }

        private void AddNodeToGraph(CodeGraphNode _node)
        {
            _node.typeName = name.GetType().AssemblyQualifiedName; 

            CodeGraphEditorNode editorNode = new CodeGraphEditorNode(_node, m_serializedObject);
            editorNode.SetPosition(_node.position);
            graphNodes.Add(editorNode);
            nodeDictionary.Add(_node.id, editorNode);

            AddElement(editorNode);
        }

        private void Bind()
        {
            //Debug.Log("Bind");
            m_serializedObject.Update();
            this.Bind(m_serializedObject);
        }
    }
}
