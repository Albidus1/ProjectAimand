using System;
using UnityEngine;

namespace CodeGraph
{
    public class NodeInformationAttribute : Attribute
    {
        public string title => m_nodeTile;
        public string menuItem => m_menuItem;

        public bool flowInput => m_hasFlowInput;
        public bool flowOutput => m_hasFlowOutput;

        private string m_nodeTile;
        private string m_menuItem;

        private bool m_hasFlowInput;
        private bool m_hasFlowOutput;



        public NodeInformationAttribute(string _title, string _menuItem = "", bool _hasFlowInput = true, bool _hasFlowOutput = true)
        {
            m_nodeTile = _title;
            m_menuItem = _menuItem;
            m_hasFlowInput = _hasFlowInput;
            m_hasFlowOutput = _hasFlowOutput;
        }
    }
}
