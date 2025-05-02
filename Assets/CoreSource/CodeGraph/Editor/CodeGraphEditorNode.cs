using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Graphs;
using UnityEditor.UIElements;
using UnityEngine;

namespace CodeGraph.Editor
{
    public class CodeGraphEditorNode : UnityEditor.Experimental.GraphView.Node
    {
        public CodeGraphNode node => m_graphNode;
        public List<Port> ports => m_ports;

        private CodeGraphNode m_graphNode;
        private Port m_outputPort;
        private List<Port> m_ports;
        private SerializedProperty m_serializeProperty;
        private SerializedObject m_serializedObject;

        public CodeGraphEditorNode(CodeGraphNode _node, SerializedObject _codeGraphObejct) 
        {
            this.AddToClassList("code-graph-node");

            m_graphNode = _node;
            m_serializedObject = _codeGraphObejct;

            Type typeInfo = _node.GetType();
            NodeInformationAttribute info = typeInfo.GetCustomAttribute<NodeInformationAttribute>();

            title = info.title;

            m_ports = new List<Port>();

            string[] depths = info.menuItem.Split('/');
            foreach (string depth in depths)
            {
                this.AddToClassList(depth.ToLower().Replace(' ', '-'));
            }

            this.name = typeInfo.Name;

            if (info.flowOutput)
            {
                CreateFlowOutputPort();
            }

            if (info.flowInput)
            {
                CreateFlowInputPort();
            }

            foreach (FieldInfo property in typeInfo.GetFields())
            {
                if (property.GetCustomAttribute<ExposedPropertyAttribute>() is ExposedPropertyAttribute exposedProperty)
                {
                    //Debug.Log(property.Name);
                    PropertyField field = DrawProperty(property.Name);

                    //field.RegisterValueChangeCallback(OnFieldChangeCallback);
                }
            }

            RefreshExpandedState();
        }

        private void FetchSerializedProperty()
        {
            SerializedProperty nodes = m_serializedObject.FindProperty("m_nodes");

            if (nodes.isArray)
            {
                int size = nodes.arraySize;

                for (int i = 0; i < size; i++)
                {
                    var element = nodes.GetArrayElementAtIndex(i);
                    var elementId = element.FindPropertyRelative("m_guid");

                    if (elementId.stringValue == m_graphNode.id)
                    {
                        m_serializeProperty = element;
                    }
                }
            }
        }

        private PropertyField DrawProperty(string _propertyName)
        {
            if (m_serializeProperty == null)
            {
                FetchSerializedProperty();
            }

            SerializedProperty prop = m_serializeProperty.FindPropertyRelative(_propertyName);
            PropertyField field = new PropertyField(prop);
            field.bindingPath = prop.propertyPath;
            extensionContainer.Add(field);

            return field;
        }


        private void CreateFlowInputPort()
        {
            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(PortTypes.FlowPort));
            inputPort.portName = "In";
            inputPort.tooltip = "입력";
            m_ports.Add(inputPort);
            inputContainer.Add(inputPort);
        }

        private void CreateFlowOutputPort()
        {
            m_outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(PortTypes.FlowPort));
            m_outputPort.portName = "Out";
            m_outputPort.tooltip = "출력";
            m_ports.Add(m_outputPort);
            outputContainer.Add(m_outputPort);
        }

        public void SavePosition()
        {
            m_graphNode.SetPosition(GetPosition());
        }
    }
}
