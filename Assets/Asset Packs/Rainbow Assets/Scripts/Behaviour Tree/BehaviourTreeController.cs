using UnityEngine;

namespace RainbowAssets.BehaviourTree
{
    /// <summary>
    /// Controls the execution of a behaviour tree within a MonoBehaviour.
    /// </summary>
    public class BehaviourTreeController : MonoBehaviour
    {
        /// <summary>
        /// The behaviour tree instance controlled by this component.
        /// </summary>
        [SerializeField] BehaviourTree behaviourTree;

        /// <summary>
        /// Gets the current behaviour tree instance.
        /// </summary>
        /// <returns>The controlled behaviour tree.</returns>
        public BehaviourTree GetBehaviourTree()
        {
            return behaviourTree;
        }

        // LIFECYCLE METHODS

        void Awake()
        {
            behaviourTree = behaviourTree.Clone();
        }

        void Start()
        {
            behaviourTree.Bind(this);
        }

        void Update()
        {
            behaviourTree.Tick();
        }
    }
}