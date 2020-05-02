using System;
using System.Collections.Generic;
using System.IO;
using Narupa.Visualisation.Components;
using Narupa.Visualisation.Node.Adaptor;
using Narupa.Visualisation.Property;
using UnityEditor;

namespace Narupa.Visualisation.Editor
{
    /// <summary>
    /// Generates a graph of the currently selected visualiser as graph in the DOT
    /// language, suitable for visualisation using GraphViz.
    /// </summary>
    internal static class GraphVizVisualisationGraph
    {
        [MenuItem("Narupa/Generate GraphVIZ")]
        internal static void GenerateGraphViz()
        {
            var root = VisualiserPreviewTool.Instance?.CurrentVisualiser;
            if (root == null)
                root = Selection.activeGameObject;
            if (root == null)
                return;

            var file = new StringWriter();

            file.WriteLine("digraph G {");

            file.WriteLine("rankdir=RL");

            var connections = new List<(long, long)>();

            var gotoChildren = true;

            var nodes = root.GetComponent<VisualisationSceneGraph>();
            var properties = new Dictionary<long, IReadOnlyProperty>();
            foreach (var node in nodes.Nodes)
                DrawObject(node, file, connections, properties);

            if(VisualiserPreviewTool.Instance?.FrameAdaptor is FrameAdaptorNode adaptor)
                DrawObject(adaptor, file, connections, properties);
            
            foreach (var connection in connections)
                file.WriteLine($"{connection.Item1} -> {connection.Item2};");

            file.WriteLine("}");

            var str = file.ToString();

            EditorGUIUtility.systemCopyBuffer = str;
        }

        internal static void DrawObject(IVisualisationNode node,
                                        TextWriter file,
                                        List<(long, long)> connections,
                                        Dictionary<long, IReadOnlyProperty> properties)
        {
            file.WriteLine($"subgraph cluster_{GetId(node)} {{");
            file.WriteLine($"label = \"{node.GetType()}\"");

            foreach (var (name, prop) in node.AsProvider().GetProperties())
            {
                DrawProperty(name, prop, file);
                FindConnections(prop, connections, properties);
            }

            file.WriteLine("}");
        }

        private static void DrawProperty(string name, IReadOnlyProperty prop, TextWriter file)
        {
            var label = name;
            if (prop.HasValue && prop.PropertyType.IsArray)
                label +=
                    $" {prop.PropertyType.GetElementType().Name}[{(prop.Value as Array).Length}]";
            var color = prop.HasValue ? "green" : "red";
            file.WriteLine($"{GetId(prop)} [label=\"{label}\" color={color} shape=box];");
        }

        internal static IEnumerable<(string, IReadOnlyProperty)> GetProperties(
            VisualisationComponent child)
        {
            foreach (var prop in child.GetProperties())
            {
                yield return prop;
                if (prop.property is IFilteredProperty filter)
                    yield return (prop.name, filter.SourceProperty);
            }
        }

        internal static void FindConnections(IReadOnlyProperty prop,
                                             List<(long, long)> connections,
                                             Dictionary<long, IReadOnlyProperty> properties)
        {
            if (prop is IProperty property && property.HasLinkedProperty)
            {
                var linked = property.LinkedProperty;
                var conn = (GetId(prop), GetId(linked));
                if (!connections.Contains(conn))
                    connections.Add(conn);
                properties[GetId(prop)] = prop;
                properties[GetId(linked)] = linked;
                FindConnections(linked, connections, properties);
            }

            if (prop is IFilteredProperty filtered)
            {
                var conn1 = (GetId(prop), GetId(filtered.SourceProperty));
                if (!connections.Contains(conn1))
                    connections.Add(conn1);
                var conn2 = (GetId(prop), GetId(filtered.FilterProperty));
                if (!connections.Contains(conn2))
                    connections.Add(conn2);
            }
        }

        internal static long GetId(object obj)
        {
            return (long) int.MaxValue + (long) obj.GetHashCode();
        }
    }
}