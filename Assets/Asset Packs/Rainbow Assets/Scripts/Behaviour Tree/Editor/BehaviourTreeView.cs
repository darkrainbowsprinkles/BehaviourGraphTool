using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;

namespace RainbowAssets.BehaviourTree.Editor
{
    /// <summary>
    /// The visual representation and interaction handler for behavior trees in the editor.
    /// </summary>
    public class BehaviourTreeView : GraphView
    {
        /// <summary>
        /// UXML factory class for creating BehaviorTreeView instances from UI Builder.
        /// </summary>
        new class UxmlFactory : UxmlFactory<BehaviourTreeView, UxmlTraits> { }

        /// <summary>
        /// Reference to the currently displayed Behavior Tree asset.
        /// </summary>
        BehaviourTree behaviourTree;

        /// <summary>
        /// Initializes a new instance of the Behavior Tree view.
        /// Sets up visual styles and interaction manipulators.
        /// </summary>
        public BehaviourTreeView()
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(BehaviourTreeEditor.path + "BehaviourTreeEditor.uss");
            styleSheets.Add(styleSheet);

            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        /// <summary>
        /// Refreshes the view with a new or updated Behavior Tree.
        /// </summary>
        /// <param name="behaviourTree">The Behavior Tree to display.</param>
        public void Refresh(BehaviourTree behaviourTree)
        {
            this.behaviourTree = behaviourTree;

            graphViewChanged -= OnGraphViewChanged;

            DeleteElements(graphElements);

            graphViewChanged += OnGraphViewChanged;

            if (behaviourTree != null)
            {
                foreach (var node in behaviourTree.GetNodes())
                {
                    CreateNodeView(node);
                }

                foreach (var node in behaviourTree.GetNodes())
                {
                    foreach (var child in behaviourTree.GetChildren(node))
                    {
                        if (child == null)
                        {
                            continue;
                        }

                        CreateEdge(node, child);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the visual status indicators for all nodes.
        /// </summary>
        public void DrawStatus()
        {
            foreach (var node in nodes)
            {
                if (node is NodeView nodeView)
                {
                    nodeView.DrawStatus();
                }
            }
        }

        /// <summary>
        /// Gets all ports that are compatible with the given start port.
        /// </summary>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            foreach (var endPort in ports)
            {
                if (endPort.direction == startPort.direction)
                {
                    continue;
                }

                if (endPort.node == startPort.node)
                {
                    continue;
                }

                compatiblePorts.Add(endPort);
            }

            return compatiblePorts;
        }

        /// <summary>
        /// Builds the contextual menu for node creation.
        /// </summary>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (!Application.isPlaying)
            {
                base.BuildContextualMenu(evt);
                
                var nodeTypes = TypeCache.GetTypesDerivedFrom<Node>();

                foreach (var type in nodeTypes)
                {
                    if (type.IsAbstract)
                    {
                        continue;
                    }

                    if (type == typeof(RootNode))
                    {
                        continue;
                    }

                    Vector2 mousePosition = viewTransform.matrix.inverse.MultiplyPoint(evt.localMousePosition);

                    evt.menu.AppendAction($"Create Node/{type.Name} ({type.BaseType.Name})", a => CreateNode(type, mousePosition));
                }
            }
        }

        /// <summary>
        /// Gets the NodeView associated with a specific node ID.
        /// </summary>
        NodeView GetNodeView(string nodeID)
        {
            return GetNodeByGuid(nodeID) as NodeView;
        }

        /// <summary>
        /// Creates a visual NodeView for a Behavior Tree node.
        /// </summary>
        void CreateNodeView(Node node)
        {
            NodeView nodeView = new(node);
            AddElement(nodeView);
        }

        /// <summary>
        /// Creates a new node in the Behavior Tree and its visual representation.
        /// </summary>
        void CreateNode(Type type, Vector2 position)
        {
            Node newNode = behaviourTree.CreateNode(type, position);
            CreateNodeView(newNode);
        }

        /// <summary>
        /// Removes a node from both the view and the Behavior Tree.
        /// </summary>
        void RemoveNode(NodeView nodeView)
        {
            behaviourTree.RemoveNode(nodeView.GetNode());
        }

        /// <summary>
        /// Creates a visual edge between two nodes.
        /// </summary>
        void CreateEdge(Node parent, Node child)
        {
            NodeView parentView = GetNodeView(parent.GetUniqueID());
            NodeView childView = GetNodeView(child.GetUniqueID());
            AddElement(parentView.ConnectTo(childView));
        }

        /// <summary>
        /// Establishes parent-child relationship in the Behavior Tree when an edge is created.
        /// </summary>
        void AddChild(Edge edge)
        {
            NodeView parentView = edge.output.node as NodeView;
            NodeView childView = edge.input.node as NodeView;

            Node parentNode = parentView.GetNode();
            Node childNode = childView.GetNode();

            if (parentNode is DecoratorNode decoratorNode)
            {
                decoratorNode.SetChild(childNode);
            }

            if (parentNode is CompositeNode compositeNode)
            {
                compositeNode.AddChild(childNode);
            }
        }

        /// <summary>
        /// Removes parent-child relationship when an edge is deleted.
        /// </summary>
        void RemoveChild(Edge edge)
        {
            NodeView parentView = edge.output.node as NodeView;
            NodeView childView = edge.input.node as NodeView;

            Node parentNode = parentView.GetNode();
            Node childNode = childView.GetNode();

            if (parentNode is DecoratorNode decoratorNode)
            {
                decoratorNode.UnsetChild();
            }

            if (parentNode is CompositeNode compositeNode)
            {
                compositeNode.RemoveChild(childNode);
            }
        }

        /// <summary>
        /// Handles changes to the graph view (edge creation, element removal, etc.)
        /// </summary>
        GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.edgesToCreate != null)
            {
                foreach (var edge in graphViewChange.edgesToCreate)
                {
                    AddChild(edge);
                }
            }

            if (graphViewChange.elementsToRemove != null)
            {
                foreach(var element in graphViewChange.elementsToRemove)
                {
                    if (element is NodeView nodeView)
                    {
                        RemoveNode(nodeView);
                    }
                    else if (element is Edge edge)
                    {
                        RemoveChild(edge);
                    }
                }
            }

            if (graphViewChange.movedElements != null)
            {
                foreach (var node in nodes)
                {
                    if (node is NodeView nodeView)
                    {
                        nodeView.SortChildren();
                    }
                }
            }

            return graphViewChange;
        }

        /// <summary>
        /// Handles undo/redo operations by refreshing the view.
        /// </summary>
        void OnUndoRedo()
        {
            Refresh(behaviourTree);
        }
    }
}