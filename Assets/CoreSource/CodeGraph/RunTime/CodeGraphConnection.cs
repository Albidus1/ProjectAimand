using UnityEngine;

namespace CodeGraph
{
    [System.Serializable]
    public struct CodeGraphConnection
    {
        public CodeGraphConnectionPort inputPort;
        public CodeGraphConnectionPort outputPort;



        public CodeGraphConnection(CodeGraphConnectionPort _input, CodeGraphConnectionPort _output)
        {
            inputPort = _input;
            outputPort = _output;
        }

        public CodeGraphConnection(string _inputPortId, int _inputPortIndex, string _outputPortId, int _outputPortIndex)
        {
            inputPort = new CodeGraphConnectionPort(_inputPortId, _inputPortIndex);
            outputPort = new CodeGraphConnectionPort(_outputPortId, _outputPortIndex);
        }
    }

    [System.Serializable]
    public struct CodeGraphConnectionPort
    {
        public string nodeId;
        public int portIndex;

        public CodeGraphConnectionPort(string _id, int _portIndex)
        {
            nodeId = _id;
            portIndex = _portIndex;
        }
    }
}
