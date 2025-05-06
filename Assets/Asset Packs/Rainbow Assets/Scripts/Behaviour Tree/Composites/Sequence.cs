using System.Linq;

namespace RainbowAssets.BehaviourTree.Composites
{
    /// <summary>
    /// A composite behavior tree node that ticks its children in sequence until one fails or all succeed.
    /// </summary>
    public class Sequence : CompositeNode
    {
        /// <summary>
        /// Index of the currently active child node.
        /// </summary>
        int currentChildIndex = 0;

        protected override void OnEnter()
        {
            currentChildIndex = 0;
        }

        protected override Status OnTick()
        {
            Status currentStatus = GetChild(currentChildIndex).Tick();

            switch (currentStatus)
            {
                case Status.Running:
                    return Status.Running;

                case Status.Success:
                    currentChildIndex++;
                    break;

                case Status.Failure:
                    return Status.Failure;
            }

            if (currentChildIndex >= GetChildren().Count())
            {
                return Status.Success;
            }

            return Status.Running;
        }

        protected override void OnExit() { }
    }
}