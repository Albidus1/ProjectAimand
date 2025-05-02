using UnityEngine;
using System;


namespace CodeGraph
{
    [System.Serializable]
    public class CodeGraphNode
    {
        [SerializeField] private string m_guid;
        [SerializeField] private Rect m_position;

        public string typeName;
        public string id => m_guid;
        public Rect position => m_position;

        

        public CodeGraphNode()
        {
            NewGUID();
        }

        private void NewGUID()
        {
            m_guid = Guid.NewGuid().ToString();
        }

        public void SetPosition(Rect _position)
        {
            m_position = _position;
        }

        public virtual string OnProcess(CodeGraphAsset _currentGraph)
        {
            CodeGraphNode nextNodeInFlow = _currentGraph.GetNodeFromOutput(m_guid, 0);

            if (nextNodeInFlow != null)
            {
                return nextNodeInFlow.id;
            }

            return string.Empty;
        }
    }
}
