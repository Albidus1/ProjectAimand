using UnityEngine;

namespace CodeGraph
{
    [NodeInformation("Debug Log", "Debug/Debug Log Console")]
    public class DebugLogNode : CodeGraphNode
    {
        [ExposedProperty]
        public string logMessage = "디버그 로그로 이동함";

        public override string OnProcess(CodeGraphAsset _currentGraph)
        {
            Debug.Log(logMessage);

            return base.OnProcess(_currentGraph);
        }
    }
}
