using System.Collections.Generic;
using RainbowAssets.Utils;
using UnityEditor;
using UnityEngine;

namespace RainbowAssets.BehaviourTree
{
    /// <summary>
    /// Abstract base class for all composite nodes in the behavior tree.
    /// </summary>
    public abstract class CompositeNode : Node
    {
        /// <summary>
        /// Optional condition that can abort the node's execution.
        /// </summary>
        [SerializeField] Condition abortCondition;

        /// <summary>
        /// List of child nodes (hidden in inspector).
        /// </summary>
        [HideInInspector, SerializeField] List<Node> children = new();

        public override Node Clone()
        {
            CompositeNode clone = Instantiate(this);
            clone.children.Clear();
            foreach (var child in children)
            {
                clone.children.Add(child.Clone());
            }
            return clone;
        }

        /// <summary>
        /// Gets all child nodes.
        /// </summary>
        /// <returns>Enumerable collection of child nodes.</returns>
        public IEnumerable<Node> GetChildren()
        {
            return children;
        }

        /// <summary>
        /// Gets a specific child node by index.
        /// </summary>
        /// <param name="index">Index of the child node.</param>
        /// <returns>The child node at specified index.</returns>
        public Node GetChild(int index)
        {
            return children[index];
        }

        /// <summary>
        /// Sorts children by their visual position in the editor.
        /// </summary>
        public void SortChildrenByPosition()
        {
            children.Sort(ComparePosition);
        }

        /// <summary>
        /// Sorts children by their execution priority (highest first).
        /// </summary>
        public void SortChildrenByPriority()
        {
            children.Sort(ComparePriority);
        }

        /// <summary>
        /// Randomizes the order of child nodes.
        /// </summary>
        public void ShuffleChildren()
        {
            int currentChild = children.Count;
            while(currentChild > 1)
            {
                currentChild--;
                int randomIndex = new System.Random().Next(currentChild + 1);
                Node randomNode = children[randomIndex];
                children[randomIndex] = children[currentChild];
                children[currentChild] = randomNode;
            }
        }

        public override void Abort()
        {
            foreach(var child in children)
            {
                child.Abort();
            }
            base.Abort();
        }

        public override Status Tick()
        {
            if(!abortCondition.IsEmpty() && 
            abortCondition.Check(controller.GetComponents<IPredicateEvaluator>()))
            {
                Abort();
                return Status.Failure;
            }
            return base.Tick();
        }

    #if UNITY_EDITOR
        /// <summary>
        /// Adds a child node (Editor only).
        /// </summary>
        /// <param name="child">Node to add as child.</param>
        public void AddChild(Node child)
        {
            Undo.RecordObject(this, "Child added");
            children.Add(child);
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Removes a child node (Editor only).
        /// </summary>
        /// <param name="childToRemove">Node to remove from children.</param>
        public void RemoveChild(Node childToRemove)
        {
            Undo.RecordObject(this, "Child removed");
            children.Remove(childToRemove);
            EditorUtility.SetDirty(this);
        }
    #endif

        /// <summary>
        /// Compares node positions for sorting (left-to-right).
        /// </summary>
        int ComparePosition(Node leftNode, Node rightNode)
        {
            return leftNode.GetPosition().x < rightNode.GetPosition().x ? -1 : 1;
        }

        /// <summary>
        /// Compares node priorities for sorting (highest first).
        /// </summary>
        int ComparePriority(Node leftNode, Node rightNode)
        {   
            return leftNode.GetPriority() < rightNode.GetPriority() ? 1 : -1;
        }
    }
}