using System.Linq;
using UnityEngine;

namespace RainbowAssets.BehaviourTree.Composites
{
    /// <summary>
    /// A composite node that selects a child to tick based on a selection strategy.
    /// </summary>
    public class Selector : CompositeNode
    {
        /// <summary>
        /// The strategy used to select the order in which children are evaluated.
        /// </summary>
        [SerializeField] SelectionType selectionType;

        /// <summary>
        /// The index of the current child being evaluated.
        /// </summary>
        int currentChildIndex = 0;

        /// <summary>
        /// Defines how children are selected for evaluation.
        /// </summary>
        enum SelectionType
        {
            /// <summary>
            /// Children are evaluated in the order they are listed until one succeeds.
            /// </summary>
            FirstToBeSuccessful,

            /// <summary>
            /// Children are sorted by priority before evaluation.
            /// </summary>
            ByPriority,

            /// <summary>
            /// Children are shuffled into a random order before evaluation.
            /// </summary>
            Random
        }

        /// <summary>
        /// Sets the child evaluation order based on the selected selection type.
        /// </summary>
        void SetSelection()
        {
            switch (selectionType)
            {
                case SelectionType.FirstToBeSuccessful:
                    break;

                case SelectionType.ByPriority:
                    SortChildrenByPriority();
                    break;

                case SelectionType.Random:
                    ShuffleChildren();
                    break;
            }
        }

        protected override void OnEnter()
        {
            currentChildIndex = 0;

            SetSelection();
        }

        protected override Status OnTick()
        {
            Status currentStatus = GetChild(currentChildIndex).Tick();

            switch (currentStatus)
            {
                case Status.Running:
                    return Status.Running;

                case Status.Success:
                    return Status.Success;

                case Status.Failure:
                    currentChildIndex++;
                    break;
            }

            if (currentChildIndex >= GetChildren().Count())
            {
                return Status.Failure;
            }

            return Status.Running;
        }

        protected override void OnExit() { }
    }

}