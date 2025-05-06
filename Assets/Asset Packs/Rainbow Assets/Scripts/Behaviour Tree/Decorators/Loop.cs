namespace RainbowAssets.BehaviourTree
{
    /// <summary>
    /// A decorator node that continuously executes its child node in a loop.
    /// </summary>
    public class Loop : DecoratorNode
    {
        protected override void OnEnter() { }

        protected override Status OnTick()
        {
            GetChild().Tick();
            return Status.Running;
        }

        protected override void OnExit() { }
    }
}