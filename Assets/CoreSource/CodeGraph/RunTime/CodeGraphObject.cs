using System;
using UnityEngine;

namespace CodeGraph
{
    public class CodeGraphObject : MonoBehaviour
    {
        [SerializeField] private CodeGraphAsset m_graphAsset;
        private CodeGraphAsset m_graphInstance;



        private void OnEnable()
        {
            m_graphInstance = Instantiate(m_graphAsset);
            ExecuteAsset();
        }

        private void ExecuteAsset()
        {
            m_graphInstance.Initialize(this.gameObject);

            CodeGraphNode startNode = m_graphInstance.GetStartNode();
            ProcessAndMoveToNextNode(startNode);
        }

        private void ProcessAndMoveToNextNode(CodeGraphNode startNode)
        {
            string nextNodeId = startNode.OnProcess(m_graphInstance);

            if (false == string.IsNullOrEmpty(nextNodeId))
            {
                CodeGraphNode node = m_graphInstance.GetNode(nextNodeId);

                ProcessAndMoveToNextNode(node);
            }
        }
    }
}
