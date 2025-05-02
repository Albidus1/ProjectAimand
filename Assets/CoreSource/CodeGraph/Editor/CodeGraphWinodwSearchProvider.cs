using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CodeGraph.Editor
{
    public struct SearchContextElement
    {
        public object target {  get; private set; }
        public string title { get; private set; }

        public SearchContextElement(object _target, string _title)
        {
            target = _target;
            title = _title;
        }
    }


    public class CodeGraphWinodwSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        public CodeGraphView graph;
        public VisualElement target;

        public static List<SearchContextElement> elements;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext _context)
        {
            List<SearchTreeEntry> tree = new List<SearchTreeEntry>();
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Nodes"), 0));

            elements = new List<SearchContextElement>();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.CustomAttributes.ToString() != null)
                    {
                        var attribute = type.GetCustomAttribute(typeof(NodeInformationAttribute));

                        if (attribute != null)
                        {
                            NodeInformationAttribute att = attribute as NodeInformationAttribute;

                            var node = Activator.CreateInstance(type);

                            if (string.IsNullOrEmpty(att.menuItem))
                            {
                                continue;
                            }

                            elements.Add(new SearchContextElement(node, att.menuItem));
                        }
                    }
                }
            }

            elements.Sort((entry1, entry2) =>
            {
                string[] splits1 = entry1.title.Split('/');
                string[] splits2 = entry2.title.Split('/');

                for (int i = 0;  i < splits1.Length; i++)
                {
                    if (i >= splits2.Length)
                    {
                        return 1;
                    }

                    int value = splits1[i].CompareTo(splits2[i]);

                    if (value != 0)
                    {
                        if (splits1.Length !=  splits2.Length && (i == splits1.Length - 1 || i == splits2.Length - 1))
                        {
                            return splits1.Length < splits2.Length ? 1 : -1;
                        }

                        return value;
                    }
                }

                return 0;
            });


            List<string> groups = new List<string>();

            foreach (SearchContextElement element in elements)
            {
                string[] entryTitles = element.title.Split('/');
                string groupName = "";

                for (int i = 0; i < entryTitles.Length - 1; i++)
                {
                    groupName += entryTitles[i];

                    if (false == groups.Contains(groupName))
                    {
                        tree.Add(new SearchTreeGroupEntry(new GUIContent(entryTitles[i]), i + 1));
                        groups.Add(groupName);
                    }

                    groupName += "/";
                }

                Debug.Log(entryTitles.Last());
                SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(entryTitles.Last()));
                entry.level = entryTitles.Length;
                entry.userData = new SearchContextElement(element.target, element.title);
                tree.Add(entry);
            }

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry _entry, SearchWindowContext _context)
        {
            var windowMousePosition = graph.ChangeCoordinatesTo(graph, _context.screenMousePosition - graph.window.position.position);
            var graphMousePosition = graph.contentViewContainer.WorldToLocal(windowMousePosition);

            SearchContextElement element = (SearchContextElement)_entry.userData;

            CodeGraphNode node = (CodeGraphNode)element.target;
            node.SetPosition(new Rect(graphMousePosition, new Vector2()));
            graph.Add(node);

            return true;
        }
    }
}
