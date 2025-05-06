namespace RainbowAssets.BehaviourTree
{
    /// <summary>
    /// Represents the possible execution states of the behavior tree node.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// The node is currently executing and has not completed.
        /// </summary>
        Running,

        /// <summary>
        /// The node has completed its execution successfully.
        /// </summary>
        Success,

        /// <summary>
        /// The node has completed its execution unsuccessfully.
        /// </summary>
        Failure
    }
}