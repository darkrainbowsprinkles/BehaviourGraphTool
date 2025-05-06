using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RainbowAssets.BehaviourTree.Editor
{
    /// <summary>
    /// Visual representation of a behavior tree node in the editor graph view.
    /// </summary>
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        /// <summary>
        /// The behavior tree node this view represents.
        /// </summary>
        Node node;
        /// <summary>
        /// Output port for connecting to child nodes.
        /// </summary>
        Port outputPort;
        /// <summary>
        /// Input port for connecting to parent nodes.
        /// </summary>
        Port inputPort;

        /// <summary>
        /// Initializes a new node view for the specified behavior tree node.
        /// </summary>
        /// <param name="node">The behavior tree node to visualize.</param>
        public NodeView(Node node) : base(BehaviourTreeEditor.path + "NodeView.uxml")
        {
            this.node = node;

            viewDataKey = node.GetUniqueID();
            
            title = node.name;

            style.left = node.GetPosition().x;
            style.top = node.GetPosition().y;

            CreatePorts();
            SetCapabilites();
            SetStyle();
            BindDescription();
        }

        /// <summary>
        /// Gets the underlying behavior tree node.
        /// </summary>
        /// <returns>The behavior tree node being visualized.</returns>
        public Node GetNode()
        {
            return node;
        }

        /// <summary>
        /// Creates a visual edge connection to another node view.
        /// </summary>
        /// <param name="child">The child node view to connect to.</param>
        /// <returns>The created edge between nodes.</returns>
        public Edge ConnectTo(NodeView child)
        {
            return outputPort.ConnectTo(child.inputPort);
        }

        /// <summary>
        /// Sorts child nodes based on their visual position.
        /// </summary>
        public void SortChildren()
        {
            CompositeNode compositeNode = node as CompositeNode;

            if(compositeNode != null)
            {
                compositeNode.SortChildrenByPosition();
            }
        }

        /// <summary>
        /// Updates the visual status indicator based on the node's runtime state.
        /// </summary>
        public void DrawStatus()
        {
            RemoveFromClassList("runningStatus");
            RemoveFromClassList("successStatus");
            RemoveFromClassList("failureStatus");

            if (Application.isPlaying)
            {
                switch (node.GetStatus())
                {
                    case Status.Running:
                        AddToClassList("runningStatus");
                        break;

                    case Status.Success:
                        AddToClassList("successStatus");
                        break;

                    case Status.Failure:
                        AddToClassList("failureStatus");
                        break;
                }
            }
        }

        /// <summary>
        /// Updates the node's position in both the view and the underlying data.
        /// </summary>
        /// <param name="newPos">The new position rectangle.</param>
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            node.SetPosition(new Vector2(newPos.x, newPos.y));
        }

        /// <summary>
        /// Handles node selection events in the editor.
        /// </summary>
        public override void OnSelected()
        {
            base.OnSelected();

            if (node is not RootNode)
            {
                Selection.activeObject = node;
            }
        }

        /// <summary>
        /// Configures node interaction capabilities based on node type.
        /// </summary>
        void SetCapabilites()
        {
            if (node is RootNode)
            {
                capabilities -= Capabilities.Deletable;
            }
        }

        /// <summary>
        /// Creates appropriate input and output ports based on node type.
        /// </summary>
        void CreatePorts()
        {
            if (node is not RootNode)
            {
                inputPort = GetPort(Direction.Input, Port.Capacity.Single);
            }

            if (node is DecoratorNode)
            {
                outputPort = GetPort(Direction.Output, Port.Capacity.Single);
            }

            if (node is CompositeNode)
            {
                outputPort = GetPort(Direction.Output, Port.Capacity.Multi);
            }
        }

        /// <summary>
        /// Creates a new port with specified direction and capacity.
        /// </summary>
        /// <param name="direction">Port direction (Input/Output).</param>
        /// <param name="capacity">Port connection capacity.</param>
        /// <returns>The created port instance.</returns>
        Port GetPort(Direction direction, Port.Capacity capacity)
        {
            Port newPort = InstantiatePort(Orientation.Vertical, direction, capacity, typeof(bool));

            if (direction == Direction.Input)
            {
                inputContainer.Add(newPort);
            }

            if (direction == Direction.Output)
            {
                outputContainer.Add(newPort);
            }

            return newPort;
        }

        /// <summary>
        /// Applies visual styles based on node type.
        /// </summary>
        void SetStyle()
        {
            if (node is RootNode)
            {
                AddToClassList("rootNode");
            }

            else if (node is DecoratorNode)
            {
                AddToClassList("decoratorNode");
            }

            if (node is CompositeNode)
            {
                AddToClassList("compositeNode");
            }

            if (node is ActionNode)
            {
                AddToClassList("actionNode");
            }
        }

        /// <summary>
        /// Binds and displays the node's description text.
        /// </summary>
        void BindDescription()
        {
            Label descriptionLabel = this.Q<Label>("description");

            descriptionLabel.text = node.GetDescription();

            if (node is not RootNode)
            {
                descriptionLabel.bindingPath = "description";
                descriptionLabel.Bind(new SerializedObject(node));
            }
        }
    }
}