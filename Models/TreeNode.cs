namespace ModelHierarchyApp.Models
{
    /// <summary>
    /// Represents a node in the jsTree structure for hierarchical data display.
    /// </summary>
    public class TreeNode
    {
        /// <summary>
        /// Gets or sets the unique identifier for the node.
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the parent node.
        /// </summary>
        public string parent { get; set; }

        /// <summary>
        /// Gets or sets the display text for the node.
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// Gets or sets the type of the node (used by jsTree, e.g., "hub", "project", "folder", "item").
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the node has children.
        /// </summary>
        public bool children { get; set; }
    }
}
