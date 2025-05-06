namespace RainbowAssets.BehaviourTree
{
    /// <summary>
    /// The root node of the behavior tree that serves as the entry point for execution.
    /// </summary>
    public class RootNode : DecoratorNode
    {
        protected override void OnEnter() { }

        protected override Status OnTick()
        {
            return GetChild().Tick();
        }

        protected override void OnExit() { }
    }
}