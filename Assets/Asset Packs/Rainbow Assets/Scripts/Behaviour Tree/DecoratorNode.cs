using RainbowAssets.Utils;
using UnityEditor;
using UnityEngine;

namespace RainbowAssets.BehaviourTree
{
    /// <summary>
    /// Abstract base class for all decorator nodes in the behavior tree.
    /// </summary>
    public abstract class DecoratorNode : Node
    {
        /// <summary>
        /// Optional condition that can abort the node's execution.
        /// </summary>
        [SerializeField] Condition abortCondition;

        /// <summary>
        /// The child node being decorated (hidden in inspector).
        /// </summary>
        [HideInInspector, SerializeField] Node child;

        public override Node Clone()
        {
            DecoratorNode clone = Instantiate(this);
            clone.child = child.Clone();
            return clone;
        }

        /// <summary>
        /// Gets the child node being decorated.
        /// </summary>
        /// <returns>The child node.</returns>
        public Node GetChild()
        {
            return child;
        }

        public override void Abort()
        {
            child.Abort();
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
        /// Sets the child node (Editor only).
        /// </summary>
        /// <param name="child">The node to set as child.</param>
        public void SetChild(Node child)
        {
            Undo.RecordObject(this, "Child Set");
            this.child = child;
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Removes the current child node (Editor only).
        /// </summary>
        public void UnsetChild()
        {
            Undo.RecordObject(this, "Child removed");
            child = null;
            EditorUtility.SetDirty(this);
        }
    #endif
    }
}   