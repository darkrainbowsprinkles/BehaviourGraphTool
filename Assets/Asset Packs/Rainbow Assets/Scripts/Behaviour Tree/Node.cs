using UnityEditor;
using UnityEngine;

namespace RainbowAssets.BehaviourTree
{
    /// <summary>
    /// Represents an abstract base class for nodes in the behaviour tree.
    /// </summary>
    public abstract class Node : ScriptableObject
    {
        /// <summary>
        /// The execution priority of the node.
        /// </summary>
        [SerializeField] int priority;

        /// <summary>
        /// The description of the node.
        /// </summary>
        [SerializeField] string description;

        /// <summary>
        /// Unique identifier for this node.
        /// </summary>
        [HideInInspector, SerializeField] string uniqueID;

        /// <summary>
        /// The position of the node in the editor.
        /// </summary>
        [HideInInspector, SerializeField] Vector2 position;

        /// <summary>
        /// Current execution status of the node.
        /// </summary>
        Status status = Status.Running;

        /// <summary>
        /// Indicates whether the node has started.
        /// </summary>
        bool started = false;

        /// <summary>
        /// Reference to the BehaviourTreeController that manages this node.
        /// </summary>
        protected BehaviourTreeController controller;

        /// <summary>
        /// Creates a new instance of this node.
        /// </summary>
        /// <returns>A cloned instance of this node.</returns>
        public virtual Node Clone()
        {
            return Instantiate(this);
        }

        /// <summary>
        /// Binds the node to a BehaviourTreeController.
        /// </summary>
        /// <param name="controller">The BehaviourTreeController to bind to.</param>
        public void Bind(BehaviourTreeController controller)
        {
            this.controller = controller;
        }

        /// <summary>
        /// Gets the unique ID of the node.
        /// </summary>
        /// <returns>The unique ID of the node.</returns>
        public string GetUniqueID()
        {
            return uniqueID;
        }

        /// <summary>
        /// Sets the unique ID of the node.
        /// </summary>
        /// <param name="uniqueID">The unique ID to assign.</param>
        public void SetUniqueID(string uniqueID)
        {
            this.uniqueID = uniqueID;
        }

        /// <summary>
        /// Gets the description of the node.
        /// </summary>
        /// <returns>The node's description.</returns>
        public string GetDescription()
        {
            return description;
        }

        /// <summary>
        /// Gets the position of the node in the editor.
        /// </summary>
        /// <returns>The position of the node.</returns>
        public Vector2 GetPosition()
        {
            return position;
        }

        /// <summary>
        /// Gets the current status of the node.
        /// </summary>
        /// <returns>The current node status.</returns>
        public Status GetStatus()
        {
            return status;
        }

        /// <summary>
        /// Gets the priority of the node.
        /// </summary>
        /// <returns>The execution priority.</returns>
        public int GetPriority()
        {
            return priority;
        }

    #if UNITY_EDITOR
        /// <summary>
        /// Sets the position of the node in the editor.
        /// </summary>
        /// <param name="position">The new position to assign.</param>
        public void SetPosition(Vector2 position)
        {
            Undo.RecordObject(this, "Node Moved");
            this.position = position;
            EditorUtility.SetDirty(this);
        }
    #endif

        /// <summary>
        /// Aborts the node, setting its status to failure and marking it as not started.
        /// </summary>
        public virtual void Abort()
        {
            started = false;
            status = Status.Failure;
        }

        /// <summary>
        /// Executes the node's logic.
        /// </summary>
        /// <returns>The resulting status of the node.</returns>
        public virtual Status Tick()
        {
            if (!started)
            {
                OnEnter();
                started = true;
            }

            status = OnTick();

            if (status == Status.Success || status == Status.Failure)
            {
                OnExit();
                started = false;
            }

            return status;
        }

        /// <summary>
        /// Called when the node is entered.
        /// </summary>
        protected abstract void OnEnter();

        /// <summary>
        /// Called every tick while the node is running.
        /// </summary>
        /// <returns>The status of the node after execution.</returns>
        protected abstract Status OnTick();

        /// <summary>
        /// Called when the node is exited.
        /// </summary>
        protected abstract void OnExit();
    }
}