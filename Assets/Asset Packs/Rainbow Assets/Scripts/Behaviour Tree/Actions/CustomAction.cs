using RainbowAssets.Utils;
using UnityEngine;

namespace RainbowAssets.BehaviourTree
{
    /// <summary>
    /// Represents a custom action node that performs a sequence of actions and evaluates status conditions.
    /// </summary>
    public class CustomAction : ActionNode
    {
        /// <summary>
        /// Actions to perform when the node is entered.
        /// </summary>
        [SerializeField] ActionData[] onEnterActions;

        /// <summary>
        /// Actions to perform on each tick while the node is active.
        /// </summary>
        [SerializeField] ActionData[] onTickActions;

        /// <summary>
        /// Actions to perform when the node is exited.
        /// </summary>
        [SerializeField] ActionData[] onExitActions;

        /// <summary>
        /// List of status conditions that determine the outcome status of this node.
        /// </summary>
        [SerializeField] StatusCondition[] statusConditions;

        /// <summary>
        /// Represents a serializable data structure for actions with parameters.
        /// </summary>
        [System.Serializable]
        class ActionData
        {
            /// <summary>
            /// The type of action to perform.
            /// </summary>
            public EAction action;

            /// <summary>
            /// The parameters associated with the action.
            /// </summary>
            public string[] parameters;
        }

        /// <summary>
        /// Represents a mapping between a status and its associated condition.
        /// </summary>
        [System.Serializable]
        class StatusCondition
        {
            /// <summary>
            /// The status to return if the condition is met.
            /// </summary>
            public Status status;

            /// <summary>
            /// The condition to evaluate.
            /// </summary>
            public Condition condition;
        }

        /// <summary>
        /// Evaluates status conditions and returns the appropriate status.
        /// </summary>
        /// <returns>The status associated with the first condition that evaluates to true, or Running by default.</returns>
        Status CheckStatus()
        {
            foreach (var statusCondition in statusConditions)
            {
                bool success = statusCondition.condition.Check(controller.GetComponents<IPredicateEvaluator>());

                if (success)
                {
                    return statusCondition.status;
                }
            }

            return Status.Running;
        }

        /// <summary>
        /// Performs a list of actions using all attached IAction components.
        /// </summary>
        /// <param name="actions">The actions to perform.</param>
        void PerformActions(ActionData[] actions)
        {
            foreach (var action in controller.GetComponents<IAction>())
            {
                foreach (var actionData in actions)
                {
                    action.DoAction(actionData.action, actionData.parameters);
                }
            }
        }

        protected override void OnEnter()
        {
            PerformActions(onEnterActions);
        }

        protected override Status OnTick()
        {
            PerformActions(onTickActions);

            return CheckStatus();
        }

        protected override void OnExit()
        {
            PerformActions(onExitActions);
        }
    }
}