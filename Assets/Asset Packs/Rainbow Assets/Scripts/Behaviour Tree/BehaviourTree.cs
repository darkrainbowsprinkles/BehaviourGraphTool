using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RainbowAssets.BehaviourTree
{
    /// <summary>
    /// Represents a behaviour tree asset that can be created and edited in the Unity Editor.
    /// </summary>
    [CreateAssetMenu(menuName = "Rainbow Assets/New Behaviour Tree")]
    public class BehaviourTree : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <summary>
        /// The root node of the behaviour tree (hidden in inspector).
        /// </summary>
        [HideInInspector, SerializeField] Node rootNode;

        /// <summary>
        /// List of all nodes in the behaviour tree (hidden in inspector).
        /// </summary>
        [HideInInspector, SerializeField] List<Node> nodes = new();

        /// <summary>
        /// Default position offset for the root node in the editor view.
        /// </summary>
        Vector2 rootNodeOffset = new(250, 0);

        /// <summary>
        /// Creates a deep copy of the behaviour tree.
        /// </summary>
        /// <returns>A new instance of the behaviour tree with cloned nodes.</returns>
        public BehaviourTree Clone()
        {
            BehaviourTree clone = Instantiate(this);
            clone.rootNode = rootNode.Clone();
            clone.nodes.Clear();
            Traverse(clone.rootNode, node => clone.nodes.Add(node));
            return clone;
        }

        /// <summary>
        /// Binds all nodes in the tree to a behaviour tree controller.
        /// </summary>
        /// <param name="controller">The controller to bind to.</param>
        public void Bind(BehaviourTreeController controller)
        {
            Traverse(rootNode, node => node.Bind(controller));
        }

        /// <summary>
        /// Gets all nodes in the behaviour tree.
        /// </summary>
        /// <returns>Enumerable collection of nodes.</returns>
        public IEnumerable<Node> GetNodes()
        {
            return nodes;
        }

        /// <summary>
        /// Gets all children of a specified node.
        /// </summary>
        /// <param name="node">The parent node.</param>
        /// <returns>Enumerable collection of child nodes.</returns>
        public IEnumerable<Node> GetChildren(Node node)
        {
            if (node is CompositeNode compositeNode)
            {
                foreach(var child in compositeNode.GetChildren())
                {
                    yield return child;
                }
            }

            if (node is DecoratorNode decoratorNode)
            {
                yield return decoratorNode.GetChild();
            }
        }

        /// <summary>
        /// Executes one tick of the behaviour tree.
        /// </summary>
        /// <returns>The status after execution (Running, Success, or Failure).</returns>
        public Status Tick()
        {
            return rootNode.Tick();
        }

    #if UNITY_EDITOR
        /// <summary>
        /// Creates a new node in the behaviour tree (Editor only).
        /// </summary>
        /// <param name="type">The type of node to create.</param>
        /// <param name="position">The position in the editor view.</param>
        /// <returns>The newly created node.</returns>
        public Node CreateNode(Type type, Vector2 position)
        {
            Node newNode = MakeNode(type, position);
            Undo.RegisterCreatedObjectUndo(newNode, "Node Created");
            Undo.RecordObject(this, "Node Added");
            nodes.Add(newNode);
            return newNode;
        }

        /// <summary>
        /// Removes a node from the behaviour tree (Editor only).
        /// </summary>
        /// <param name="nodeToRemove">The node to remove.</param>
        public void RemoveNode(Node nodeToRemove)
        {
            Undo.RecordObject(this, "Node Removed");
            nodes.Remove(nodeToRemove);
            Undo.DestroyObjectImmediate(nodeToRemove);
        }

        /// <summary>
        /// Internal method to create a new node instance (Editor only).
        /// </summary>
        Node MakeNode(Type type, Vector2 position)
        {
            Node newNode = CreateInstance(type) as Node;
            newNode.name = type.Name;
            newNode.SetUniqueID(Guid.NewGuid().ToString());
            newNode.SetPosition(position);
            return newNode;
        }
    #endif

        /// <summary>
        /// Traverses the behaviour tree recursively.
        /// </summary>
        /// <param name="node">The starting node.</param>
        /// <param name="visiter">Action to perform on each visited node.</param>
        void Traverse(Node node, Action<Node> visiter)
        {
            if (node != null)
            {
                visiter.Invoke(node);
                foreach (var child in GetChildren(node))
                {
                    Traverse(child, visiter);
                }
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
    #if UNITY_EDITOR
            if (AssetDatabase.GetAssetPath(this) != "")
            {
                foreach (var node in nodes)
                {
                    if (AssetDatabase.GetAssetPath(node) == "")
                    {
                        AssetDatabase.AddObjectToAsset(node, this);
                    }
                }

                if (rootNode == null)
                {
                    rootNode = MakeNode(typeof(RootNode), rootNodeOffset);
                    nodes.Add(rootNode);
                }
            }
    #endif
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() { }
    }
}