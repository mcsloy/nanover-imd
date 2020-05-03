using System;
using System.Collections.Generic;
using System.Linq;
using Narupa.Core.Collections;
using Narupa.Visualisation.Components;
using Narupa.Visualisation.Node.Input;
using Narupa.Visualisation.Node.Output;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Narupa.Visualisation.Editor
{
    [CustomEditor(typeof(VisualisationSubgraph))]
    public class VisualisationSubgraphEditor : UnityEditor.Editor
    {
        private ReorderableList nodes;

        private string NicefyNodeTypeName(Type type)
        {
            var itemTypeName = ObjectNames.NicifyVariableName(type.Name);
            if (itemTypeName.EndsWith(" Node"))
                itemTypeName = itemTypeName.Remove(itemTypeName.Length - 5);
            return itemTypeName;
        }


        private string NicefyNodeName(IVisualisationNode node)
        {
            var name = NicefyNodeTypeName(node.GetType());
            if (node is IInputNode input)
                name += $" '{input.Name}'";
            if (node is IOutputNode output)
                name += $" '{output.Name}'";
            return name;
        }


        private void OnEnable()
        {
            var nodesProperty = this.serializedObject.FindProperty("nodes");
            nodes = new ReorderableList(this.serializedObject, nodesProperty);
            nodes.onAddDropdownCallback = (Rect buttonRect, ReorderableList target) =>
            {
                var menu = new GenericMenu();
                foreach (var type in TypeCache.GetTypesDerivedFrom<IVisualisationNode>()
                                              .Where(type => !type.IsAbstract &&
                                                             !type.IsGenericType))
                {
                    var nspace = ObjectNames.NicifyVariableName(type.Namespace.Split('.').Last());
                    var typeName = NicefyNodeTypeName(type);
                    menu.AddItem(new GUIContent($"{nspace}/{typeName}"), false, obj =>
                    {
                        var t = (Type) obj;
                        var index = nodes.serializedProperty.arraySize;
                        nodes.serializedProperty.arraySize++;

                        var elementProp = nodes.serializedProperty.GetArrayElementAtIndex(index);
                        elementProp.managedReferenceValue =
                            (IVisualisationNode) Activator.CreateInstance(type);
                        serializedObject.ApplyModifiedProperties();
                    }, type);
                }

                menu.ShowAsContext();
            };

            var spacing = 4f;
            var headerHeight = 16f;

            var subgraph = this.serializedObject.targetObject as VisualisationSubgraph;

            nodes.drawElementCallback = (Rect rect, int nodeIndex, bool isactive, bool isfocused) =>
            {
                var nodeProperty = nodesProperty.GetArrayElementAtIndex(nodeIndex);

                var y = rect.yMin + spacing;

                if (!nodeProperty.managedReferenceFullTypename.Contains(' '))
                    return;

                var assemblyName = nodeProperty.managedReferenceFullTypename.Split(' ')[0];
                var typeName = nodeProperty.managedReferenceFullTypename.Split(' ')[1];
                var type = Type.GetType($"{typeName}, {assemblyName}");

                var itemTypeName = NicefyNodeTypeName(type);
                EditorGUI.LabelField(new Rect(rect.xMin, y, rect.width, headerHeight), itemTypeName,
                                     EditorStyles.boldLabel);

                y += headerHeight + spacing;

                var w = Mathf.Min(180f, rect.width / 2f);
                foreach (var child in GetChildren(nodeProperty))
                {
                    var h = EditorGUI.GetPropertyHeight(child);
                    EditorGUI.LabelField(new Rect(rect.xMin, y, w, 16f),
                                         new GUIContent(child.displayName));

                    var propRect = new Rect(rect.xMin + w + 2f, y, rect.width - w - 2f, h);

                    if (child.type.EndsWith("Property"))
                    {
                        var link = subgraph.GetLink(nodeIndex, child.name);
                        var isLinked = link != null;

                        var linkButtonWidth = 56f;
                        var linkToggleRect = new Rect(propRect)
                        {
                            width = linkButtonWidth
                        };
                        linkToggleRect.x = propRect.xMax - linkToggleRect.width;
                        propRect.width -= linkButtonWidth + spacing;

                        if (GUI.Button(linkToggleRect, isLinked ? "Unlink" : "Link",
                                       EditorStyles.miniButton))
                        {
                            if (isLinked)
                            {
                                subgraph.RemoveLink(nodeIndex, child.name);
                            }
                            else
                            {
                                subgraph.AddEmptyLink(nodeIndex, child.name);
                            }
                        }

                        if (isLinked)
                        {
                            var possibleSources = subgraph
                                                  .GetPossibleSourcesForLinkToField(nodeIndex, child.name)
                                                  .ToArray();

                            var names = possibleSources
                                        .Select(
                                            l => NicefyNodeName(
                                                     l.node) + " / " +
                                                 ObjectNames.NicifyVariableName(l.key)).ToArray();

                            var indexOf = possibleSources.FirstIndexOf(
                                l => l.node == link.sourceObject && l.key == link.sourceName);

                            var newIndex = EditorGUI.Popup(propRect, indexOf, names);

                            if (newIndex != indexOf)
                            {
                                var (node, key) = possibleSources[newIndex];
                                link.sourceObject = node;
                                link.sourceName = key;
                            }
                        }
                        else
                        {
                            EditorGUI.PropertyField(propRect,
                                                    child,
                                                    GUIContent.none,
                                                    true);
                        }
                    }
                    else if (type != null && GetInputOrOutputType(type) is Type t)
                    {
                        FramePropertyDrawer.DrawFrameKeyProperty(
                            propRect, child, t);
                    }
                    else
                    {
                        EditorGUI.PropertyField(propRect,
                                                child,
                                                GUIContent.none,
                                                true);
                    }


                    y += h + spacing;
                }
            };

            nodes.elementHeightCallback = (int index) =>
            {
                var prop = nodesProperty.GetArrayElementAtIndex(index);
                var h = spacing + headerHeight + spacing;
                foreach (var child in GetChildren(prop))
                {
                    h += EditorGUI.GetPropertyHeight(child, true) + spacing;
                }

                return h;
            };
        }

        /// <summary>
        /// Checks to see if the given type implements <see cref="IInputNode{TValue}"/> or <see cref="IOutputNode{TType}"/>, and if so extracts the generic type.
        /// </summary>
        private Type GetInputOrOutputType(Type nodeType)
        {
            return nodeType.GetInterfaces()
                           .FirstOrDefault(i => i.IsGenericType
                                              &&
                                                (i.GetGenericTypeDefinition() ==
                                                 typeof(IInputNode<>) ||
                                                 i.GetGenericTypeDefinition() ==
                                                 typeof(IOutputNode<>)))
                           ?.GetGenericArguments()[0];
        }

        private IEnumerable<SerializedProperty> GetChildren(SerializedProperty property)
        {
            var next = property.Copy();
            if (!next.NextVisible(false))
                next = null;

            var child = property.Copy();
            if (!child.NextVisible(true))
                yield break;

            while (next == null || child.propertyPath != next.propertyPath)
            {
                yield return child;
                if (!child.NextVisible(false))
                    yield break;
            }
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            nodes?.DoLayoutList();

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}