using UnityEngine;

namespace CodeGraph
{
    [NodeInformation("Start", "Process/Start", false, true)]
    public class StartNode : CodeGraphNode
    {
        public override string OnProcess(CodeGraphAsset _currentNode)
        {
            Debug.Log("노드 시작");

            return base.OnProcess(_currentNode);
        }
    }
}
