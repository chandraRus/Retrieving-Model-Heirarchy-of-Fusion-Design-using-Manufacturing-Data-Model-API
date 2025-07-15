namespace ModelHierarchyApp.Models
{
    /// <summary>
    /// Represents the response for a hierarchy GraphQL query.
    /// </summary>
    public class HierarchyResponse
    {
        /// <summary>
        /// Gets or sets the hierarchy data.
        /// </summary>
        public HierarchyData Data { get; set; }
    }

    /// <summary>
    /// Contains hierarchy data.
    /// </summary>
    public class HierarchyData
    {
        /// <summary>
        /// Gets or sets the component version node.
        /// </summary>
        public ComponentVersionNode ComponentVersion { get; set; }
    }

    /// <summary>
    /// Represents a component version node.
    /// </summary>
    public class ComponentVersionNode
    {
        /// <summary>
        /// Gets or sets the component version ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the component version name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets all occurrences of the component version.
        /// </summary>
        public AllOccurrences AllOccurrences { get; set; }
    }
}