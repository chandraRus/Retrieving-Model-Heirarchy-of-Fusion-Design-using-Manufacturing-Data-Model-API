namespace ModelHierarchyApp.Models
{
    /// <summary>
    /// Represents a node in a jsTree structure for UI display.
    /// </summary>
    public class JsTreeNode
    {
        /// <summary>
        /// Gets or sets the unique identifier for the node.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the display text for the node.
        /// </summary>
        public string text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the child nodes of this node.
        /// </summary>
        public List<JsTreeNode>? children { get; set; }
    }

    /// <summary>
    /// Represents a node in a hierarchical structure.
    /// </summary>
    public class HierarchyNode
    {
        /// <summary>
        /// Gets or sets the unique identifier for the hierarchy node.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the hierarchy node.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the child nodes of this hierarchy node.
        /// </summary>
        public List<HierarchyNode> Children { get; set; } = new();
    }

    /// <summary>
    /// Represents a component node in a component hierarchy.
    /// </summary>
    public class ComponentNode
    {
        /// <summary>
        /// Gets or sets the name of the component.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the child component nodes.
        /// </summary>
        public List<ComponentNode> Children { get; set; } = new();
    }
}
