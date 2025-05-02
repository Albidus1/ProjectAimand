using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



namespace CodeGraph
{
    [CreateAssetMenu(menuName = "코드 그래프/새 그래프")]
    public class CodeGraphAsset : ScriptableObject
    {
        public GameObject gameObject;

        public List<CodeGraphNode> nodes => m_nodes;
        public List<CodeGraphConnection> connections => m_connections;

        [SerializeReference] private List<CodeGraphNode> m_nodes;
        [SerializeField] private List<CodeGraphConnection> m_connections;
        private Dictionary<string, CodeGraphNode> m_nodeDirctionary;

        public CodeGraphAsset()
        { 
            m_nodes = new List<CodeGraphNode>();
            m_connections = new List<CodeGraphConnection>();      
        }

        public void Initialize(GameObject _gameObject)
        {
            gameObject = _gameObject;

            m_nodeDirctionary = new Dictionary<string, CodeGraphNode>();

            foreach (CodeGraphNode node in nodes)
            {
                m_nodeDirctionary.Add(node.id, node);
            }
        }

        public CodeGraphNode GetStartNode()
        {
            StartNode[] startNode = nodes.OfType<StartNode>().ToArray();

            if (startNode.Length == 0 )
            {
                Debug.LogError("이 그래프에 스타드 노드 없음.");
                return null;
            }

            return startNode[0];
        }

        public CodeGraphNode GetNode(string _nextNodeId)
        {
            if (m_nodeDirctionary.TryGetValue(_nextNodeId, out CodeGraphNode node))
            {
                return node;
            }

            return null;
        }

        public CodeGraphNode GetNodeFromOutput(string _outputNodeId, int _index)
        {
            foreach (CodeGraphConnection connection in m_connections)
            {
                if (connection.outputPort.nodeId == _outputNodeId &&
                    connection.outputPort.portIndex == _index)
                {
                    string nodeId = connection.inputPort.nodeId;
                    CodeGraphNode inputNode = m_nodeDirctionary[nodeId];

                    return inputNode;
                }
            }

            return null;
        }
    }
}