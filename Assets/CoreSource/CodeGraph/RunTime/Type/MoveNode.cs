using UnityEngine;



namespace CodeGraph
{
    [NodeInformation("이동", "Transform/이동")]
    public class MoveNode : CodeGraphNode
    {
        [ExposedProperty()]
        public Vector3 direction;

        public override string OnProcess(CodeGraphAsset _currentGraph)
        {
            Move(_currentGraph);

            return base.OnProcess(_currentGraph);
        }

        private void Move(CodeGraphAsset _currentGraph)
        {
            _currentGraph.gameObject.transform.position += (Vector3)direction;
        }
    }
}
