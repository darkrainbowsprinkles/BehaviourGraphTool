using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.UIElements;

namespace RainbowAssets.BehaviourTree.Editor
{
    /// <summary>
    /// The main editor window for visualizing and editing behavior trees.
    /// </summary>
    public class BehaviourTreeEditor : EditorWindow
    {
        /// <summary>
        /// Base path for editor resources.
        /// </summary>
        public const string path = "Assets/Asset Packs/Rainbow Assets/Scripts/Behaviour Tree/Editor/";

        /// <summary>
        /// The view component that renders and handles the behavior tree graph.
        /// </summary>
        BehaviourTreeView behaviourTreeView;

        /// <summary>
        /// Opens the Behavior Tree editor window from the Unity menu.
        /// </summary>
        [MenuItem("Tools/Behaviour Graph")]
        public static void ShowWindow()
        {
            GetWindow(typeof(BehaviourTreeEditor), false, "Behaviour Graph");
        }

        /// <summary>
        /// Callback for when a Behavior Tree asset is opened in Unity.
        /// </summary>
        [OnOpenAsset]
        public static bool OnBehaviourTreeOpened(int instanceID, int line)
        {
            BehaviourTree behaviourTree = EditorUtility.InstanceIDToObject(instanceID) as BehaviourTree;

            if (behaviourTree != null)
            {
                ShowWindow();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Initializes the editor GUI by loading and setting up the visual elements.
        /// </summary>
        void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path + "BehaviourTreeEditor.uxml");
            visualTree.CloneTree(root);

            behaviourTreeView = root.Q<BehaviourTreeView>();
        }

        /// <summary>
        /// Handles selection changes to automatically display selected behavior trees.
        /// </summary>
        void OnSelectionChange()
        {
            BehaviourTree behaviourTree = Selection.activeObject as BehaviourTree;

            // Check if a GameObject with BehaviorTreeController is selected
            if (Selection.activeGameObject)
            {
                if (Selection.activeGameObject.TryGetComponent<BehaviourTreeController>(out var controller))
                {
                    behaviourTree = controller.GetBehaviourTree();
                }
            }

            if (behaviourTree != null)
            {
                behaviourTreeView.Refresh(behaviourTree);
            }
        }

        // LIFECYCLE
        
        void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        /// <summary>
        /// Handles play mode changes to refresh the tree view appropriately.
        /// </summary>
        void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (behaviourTreeView != null)
            {
                if (change == PlayModeStateChange.EnteredEditMode)
                {
                    OnSelectionChange();
                }

                if (change == PlayModeStateChange.EnteredPlayMode)
                {
                    OnSelectionChange();
                }
            }
        }

        /// <summary>
        /// Regularly updates node status visualization during play mode.
        /// </summary>
        void OnInspectorUpdate()
        {
            behaviourTreeView?.DrawStatus();
        }
    }
}